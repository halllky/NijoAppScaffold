using MyApp;
using MyApp.Core.Authorization;
using MyApp.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyApp.WebApi.Authorization;

// 起動の途中で例外が起きたときも、その内容をログファイルに残すため、全体を try で囲む。
// ログの設定が終わる前（設定ファイルの読み込みなど）に起きた例外は、ログファイルには残らず、標準エラー出力にだけ出る
try {
    var builder = WebApplication.CreateBuilder(args);

    // 使っているソフトウェアを外に知らせないよう、応答に Server ヘッダーを付けない
    builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

    // HttpContextAccessor の登録 (SessionProviderInWebApi, CurrentProcessInWebApi で使用)
    builder.Services.AddHttpContextAccessor();

    // Controller の設定
    builder.Services.AddControllers();

    // 未処理例外のログ出力と、統一されたエラーレスポンス（ProblemDetails）の返却。
    // 利用者からの問い合わせをログと突き合わせられるよう、エラーレスポンスの traceId は、ログの処理1回分の識別子と同じ値にする。
    // ASP.NET Core が既定で入れる traceId は、分散トレースの識別子が優先され、ログの識別子と一致しないため、ここで上書きする
    builder.Services.AddProblemDetails(options => {
        options.CustomizeProblemDetails = context => {
            context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        };
    });
    builder.Services.AddExceptionHandler<WebExceptionHandler>();

    // 死活の確認。ロードバランサーや監視から呼ばれる。確認するのは DB への接続だけ
    builder.Services.AddHealthChecks().AddCheck<DbHealthCheck>("DB");

    // swagger。デバッグのためにこのアプリで定義されているエンドポイントを一覧する
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // アプリケーションサービス層のDI設定
    var basePath = Path.GetFullPath(builder.Environment.ContentRootPath);
    OverridedApplicationService.ConfigureServices(builder.Services, basePath, builder.Environment.IsDevelopment());

    // ログインユーザーの情報を各業務処理中で簡単に参照できるようにする
    builder.Services.AddScoped<ISessionKeyProvider, SessionProviderInWebApi>();

    // 認証・認可。セッションキーの Cookie でログインユーザーを特定し、
    // 個別に AllowAnonymous を指定したもの以外のエンドポイントはログインを必須とする
    builder.Services
        .AddAuthentication(SessionAuthenticationHandler.SCHEME)
        .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(SessionAuthenticationHandler.SCHEME, null);
    builder.Services.AddAuthorization(options => {
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    });

    // ログや作成者・更新者に記録する処理名
    builder.Services.AddScoped<ICurrentProcess, CurrentProcessInWebApi>();

    // 同じ操作が二重に処理されるのを防ぐための冪等キー
    builder.Services.AddScoped<MyApp.Core.冪等キー.IIdempotencyKeyProvider, IdempotencyKeyProviderInWebApi>();

    // HTTPリクエスト・レスポンスで使われるJSONシリアライズ設定を上記DI設定のそれに合わせる
    builder.Services.ConfigureHttpJsonOptions(options => {
        options.SerializerOptions.EditDefaultJsonSerializerOptions();
    });

    // --------------------------------

    var app = builder.Build();

    // Error 以上のログのメールでの通知は、短い時間に送る数を抑える。
    // DB に接続できないときのような横断的なエラーで、大量のメールが届くのを防ぐため
    app.Services.GetRequiredService<MyApp.Core.エラーの通知.ErrorNotifier>().EnableThrottling();

    // 静的ファイルやエラーの応答も含め、すべての応答に付けるため、ほかのどの処理よりも先に登録する
    app.UseMiddleware<SecurityHeadersMiddleware>();

    // 死活の確認。ログイン不要。
    // ロードバランサーなどから短い間隔で呼ばれ、リクエストごとのログで埋まってしまうので、ログの情報を付ける処理より先に登録して、ここで応答を返す
    app.UseHealthChecks("/health");

    if (app.Environment.IsDevelopment()) {
        // swagger。デバッグのためにこのアプリで定義されているエンドポイントを一覧する
        app.UseSwagger();
        app.UseSwaggerUI();

    } else {
        // セキュリティの設定が必要なら追加
        // app.UseHsts();

        // 本番環境では client フォルダのソースは1個の JavaScript ファイルにバンドルされて静的ファイルとして配信される
        app.UseStaticFiles(new StaticFileOptions {
            OnPrepareResponse = ctx => {
                // HTMLはサイズが小さいのと、JavaScript/CSSのバージョンアップがあったときに
                // 古いバージョンが参照されてしまうのを防ぐために、キャッシュ無効にする
                if (ctx.File.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) {
                    ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
                    ctx.Context.Response.Headers.Append("Pragma", "no-cache");
                    ctx.Context.Response.Headers.Append("Expires", "0");
                }
            }
        });
    }

    // 本番環境の DB にマイグレーションの SQL を流し忘れたまま動かさないよう、起動の時点で検知する。
    // 開発環境以外では例外にして起動を止める。例外の内容は、下の catch でログに出力される。
    // 開発環境では、起動した後にデバッグメニューから DB を作り直すことがあるので、警告にとどめる
    using (var scope = app.Services.CreateScope()) {
        var dbContext = scope.ServiceProvider.GetRequiredService<OverridedDbContext>();
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToArray();
        if (pendingMigrations.Length > 0) {
            var message = $"DB に適用されていないマイグレーションがあります: {string.Join(", ", pendingMigrations)}";
            if (app.Environment.IsDevelopment()) {
                app.Logger.LogWarning("{Message}", message);
            } else {
                throw new InvalidOperationException(message);
            }
        }
    }

    // Controller と自動生成されたクエリ・コマンドのエンドポイントに共通の処理
    app.UseMiddleware<WebLogScopeMiddleware>(); // ログ出力用情報設定。未処理例外のログにも情報が付くよう、例外処理より先に登録する
    app.UseExceptionHandler(); // 未処理例外のログ出力とエラーレスポンス返却（開発環境でも例外の内容は返さない）

    // 他のサイトからの不正なリクエスト（CSRF）の対策として、更新系の HTTP メソッドでは独自のヘッダーを必須にする。
    // 独自のヘッダーは、他のサイトのページからは CORS の許可なしに付けられないため。
    // 開発環境では、API の一覧（Swagger）から呼び出せるよう確かめない
    if (!app.Environment.IsDevelopment()) {
        app.Use(async (context, next) => {
            var method = context.Request.Method;
            var isSafeMethod = HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method);
            if (!isSafeMethod && !context.Request.Headers.ContainsKey("X-Requested-With")) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            await next(context);
        });
    }

    app.UseAuthentication(); // ログインユーザーの特定
    app.UseAuthorization(); // ログインしていないリクエストを弾く（401）

    app.MapDefaultControllerRoute();

    // サーバーのバージョン。画面のバージョンとの食い違いの確認などに使う
    app.MapGet("/api/version", () => Results.Json(new { version = AppVersion.Value })).AllowAnonymous();

    // Nijoにより自動生成されたクエリ・コマンドをHTTPエンドポイントとして公開する。
    // ログイン前に呼ばれる処理はログイン不要とする
    Operation<OperationContext>[] allowAnonymous = [
        Operations<OperationContext>.ログイン,
        Operations<OperationContext>.ログアウト,
        Operations<OperationContext>.ログイン状態確認,
    ];
    app.MapOperationEndpoints((operation, endpoint) => {
        if (allowAnonymous.Contains(operation)) {
            endpoint.AllowAnonymous();
        }
    });

    // 存在しないパスへのリクエストは index.html を返すようにし、
    // 実際にどの画面が初期表示されるかは React Router のルーティング設定で制御する。
    // 開発環境ではViteの開発サーバーがHTMLを配信するため、ここでのフォールバックは不要
    if (!app.Environment.IsDevelopment()) {
        app.MapFallbackToFile("index.html").AllowAnonymous(); // ログイン画面も含めて画面はこの HTML から始まるので、ログイン不要
    }

    // セキュリティの設定が必要なら追加
    // app.UseHttpsRedirection();

    app.Logger.LogInformation("起動します。バージョン: {Version}", AppVersion.Value);
    app.Run();

} catch (Exception ex) when (ex is not HostAbortedException) {
    // HostAbortedException は、EF Core のツールなどがホストを途中で止めるときに投げるもので、異常ではない
    // DI コンテナを作る前に起きた例外も出力できるよう、DI コンテナを介さずに出力する
    NLog.LogManager.GetCurrentClassLogger().Fatal(ex, "アプリケーションが例外で停止しました。");
    throw;

} finally {
    // 書き込まれていないログをファイルに出し切る
    NLog.LogManager.Shutdown();
}

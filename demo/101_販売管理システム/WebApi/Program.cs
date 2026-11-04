using MyApp;
using MyApp.Core.Authorization;
using MyApp.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using MyApp.WebApi.Authorization;

var builder = WebApplication.CreateBuilder(args);

// HttpContextAccessor の登録 (SessionProviderInWebApi, CurrentProcessInWebApi で使用)
builder.Services.AddHttpContextAccessor();

// Controller の設定
builder.Services.AddControllers();

// 未処理例外のログ出力と、統一されたエラーレスポンス（ProblemDetails）の返却
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<WebExceptionHandler>();

// swagger。デバッグのためにこのアプリで定義されているエンドポイントを一覧する
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// アプリケーションサービス層のDI設定
var basePath = Path.GetFullPath(builder.Environment.ContentRootPath);
OverridedApplicationService.ConfigureServices(builder.Services, basePath);

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

// HTTPリクエスト・レスポンスで使われるJSONシリアライズ設定を上記DI設定のそれに合わせる
builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.EditDefaultJsonSerializerOptions();
});

// --------------------------------

var app = builder.Build();

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

// Controller と自動生成されたクエリ・コマンドのエンドポイントに共通の処理
app.UseExceptionHandler(); // 未処理例外のログ出力とエラーレスポンス返却（開発環境でも例外の内容は返さない）
app.UseMiddleware<WebLogScopeMiddleware>(); // ログ出力用情報設定
app.UseAuthentication(); // ログインユーザーの特定
app.UseAuthorization(); // ログインしていないリクエストを弾く（401）

app.MapDefaultControllerRoute();

// Nijoにより自動生成されたクエリ・コマンドをHTTPエンドポイントとして公開する。
// ログイン前に呼ばれる処理はログイン不要とする
Operation<OperationContext>[] allowAnonymous = [
    Operation<OperationContext>.ログイン,
    Operation<OperationContext>.ログアウト,
    Operation<OperationContext>.ログイン状態確認,
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

app.Run();

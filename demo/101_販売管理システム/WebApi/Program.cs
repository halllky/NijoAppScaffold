using MyApp;
using MyApp.Core.Authorization;
using MyApp.WebApi;
using MyApp.WebApi.Authorization;

var builder = WebApplication.CreateBuilder(args);

// HttpContextAccessor の登録 (SessionProviderInWebApi, CurrentProcessInWebApi で使用)
builder.Services.AddHttpContextAccessor();

// Controller の設定
builder.Services.AddControllers();

// swagger。デバッグのためにこのアプリで定義されているエンドポイントを一覧する
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// アプリケーションサービス層のDI設定
var basePath = Path.GetFullPath(builder.Environment.ContentRootPath);
OverridedApplicationService.ConfigureServices(builder.Services, basePath);

// ログインユーザーの情報を各業務処理中で簡単に参照できるようにする
builder.Services.AddScoped<ISessionKeyProvider, SessionProviderInWebApi>();

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

    // 開発時専用エラーページ
    app.UseDeveloperExceptionPage();

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
app.UseMiddleware<WebExceptionMiddleware>(); // 未処理例外のログ出力とエラーレスポンス返却
app.UseMiddleware<WebLogScopeMiddleware>(); // ログ出力用情報設定
app.UseMiddleware<LoginAuthorizationMiddleware>(); // ログインしていないリクエストを弾く

app.MapDefaultControllerRoute();

// Nijoにより自動生成されたクエリ・コマンドをHTTPエンドポイントとして公開する。
// ログイン前に呼ばれる処理はログイン不要とする
CommandQuery<CommandQueryContext>[] allowAnonymous = [
    CommandQuery<CommandQueryContext>.ログイン,
    CommandQuery<CommandQueryContext>.ログアウト,
    CommandQuery<CommandQueryContext>.ログイン状態確認,
];
app.MapCommandQueryEndpoints((commandQuery, endpoint) => {
    if (allowAnonymous.Contains(commandQuery)) {
        endpoint.AllowAnonymous();
    }
});

// 存在しないパスへのリクエストは index.html を返すようにし、
// 実際にどの画面が初期表示されるかは React Router のルーティング設定で制御する。
// 開発環境ではViteの開発サーバーがHTMLを配信するため、ここでのフォールバックは不要
if (!app.Environment.IsDevelopment()) {
    app.MapFallbackToFile("index.html");
}

// セキュリティの設定が必要なら追加
// app.UseHttpsRedirection();

app.Run();

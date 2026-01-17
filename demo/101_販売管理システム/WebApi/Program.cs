using MyApp.Core.Authorization;
using MyApp.WebApi.Authorization;

var builder = WebApplication.CreateBuilder(args);

// HttpContextAccessor の登録 (LoginUserProviderInWebApi で使用)
builder.Services.AddHttpContextAccessor();

// Controller の設定
builder.Services.AddControllers(options => {
    options.Filters.Add<MyApp.WebApi.WebLogScope>(); // ログ出力用情報設定のフィルター
    options.Filters.Add<LoginAuthorizationFilter>();      // ログインしていないリクエストを弾くフィルター
});

// swagger。デバッグのためにこのアプリで定義されているエンドポイントを一覧する
builder.Services.AddSwaggerGen();

// CORS設定
builder.Services.AddCors(options => {
    // 開発環境ではViteからのリクエストを許可
    if (builder.Environment.IsDevelopment()) {
        options.AddDefaultPolicy(policy => {
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost") // localhostであればポート問わず許可
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // クライアント側の credentials: 'include' に対応するために必須
        });
    }
});

// アプリケーションサービス層のDI設定
var appConfig = new MyApp.OverridedApplicationConfigure();
appConfig.ConfigureServices(builder.Services);

builder.Services.AddScoped<ISessionKeyProvider, LoginUserProviderInWebApi>(); // ログインユーザー情報提供サービスをWebApi用に差し替え

// HTTPリクエスト・レスポンスで使われるJSONシリアライズ設定を上記DI設定のそれに合わせる
builder.Services.ConfigureHttpJsonOptions(options => {
    appConfig.EditDefaultJsonSerializerOptions(options.SerializerOptions);
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
    app.UseStaticFiles();
}

// CORSミドルウェアを追加
app.UseCors();

app.MapDefaultControllerRoute();

// セキュリティの設定が必要なら追加
// app.UseHttpsRedirection();

app.Run();

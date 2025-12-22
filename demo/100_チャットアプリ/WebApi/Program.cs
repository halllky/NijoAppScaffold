var builder = WebApplication.CreateBuilder(args);

// Controller の設定
builder.Services.AddControllers(options => {
    // 全エンドポイントに共通して適用される ActionFilter の設定
    options.Filters.Add<MyApp.WebApi.Base.LoggingActionFilter>();   // ログ出力
    options.Filters.Add<MyApp.WebApi.Base.GlobalExceptionFilter>(); // グローバル例外ハンドリング
});

// swagger。デバッグのためにこのアプリで定義されているエンドポイントを一覧する
builder.Services.AddSwaggerGen();

// Cookie認証の設定
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options => {
        options.LoginPath = "/api/login";
        options.LogoutPath = "/api/logout";
        options.AccessDeniedPath = "/api/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // 30日間有効
        options.SlidingExpiration = true; // アクセスのたびに有効期限を延長
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

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

// HTTPリクエスト・レスポンスで使われるJSONシリアライズ設定を上記DI設定のそれに合わせる
builder.Services.ConfigureHttpJsonOptions(options => {
    appConfig.EditDefaultJsonSerializerOptions(options.SerializerOptions);
});

// 自動生成されたエンドポイントのリクエスト・レスポンス処理の設定
builder.Services.AddScoped<MyApp.DefaultConfigurationInWebApi, MyApp.WebApi.Base.ConfigurationInWebApi>();

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

// 認証・認可ミドルウェアを追加
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

// セキュリティの設定が必要なら追加
// app.UseHttpsRedirection();

app.Run();

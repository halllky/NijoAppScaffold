var builder = WebApplication.CreateBuilder(args);

// Controller の設定
builder.Services.AddControllers(options => {
    // 全エンドポイントに共通して適用される ActionFilter の設定
    options.Filters.Add<MyApp.WebApi.Base.LoggingActionFilter>();   // ログ出力
    options.Filters.Add<MyApp.WebApi.Base.GlobalExceptionFilter>(); // グローバル例外ハンドリング
});

// swagger。デバッグのためにこのアプリで定義されているエンドポイントを一覧する
builder.Services.AddSwaggerGen();

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
    app.UseHsts();

    // 本番環境では client フォルダのソースは1個の JavaScript ファイルにバンドルされて静的ファイルとして配信される
    app.UseStaticFiles();
}

app.MapDefaultControllerRoute();

app.UseHttpsRedirection();
app.UseCors();

app.Run();

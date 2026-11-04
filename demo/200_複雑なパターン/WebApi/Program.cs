using System.Runtime.CompilerServices;
using MyApp;
using MyApp.WebApi.Base;

// このプロジェクトで宣言されている internal メンバーを、単体テストプロジェクトから参照可能にする
[assembly: InternalsVisibleTo("Demo200.UnitTest")]

// --------------------------------

var builder = WebApplication.CreateBuilder(args);

// Controller の設定
builder.Services.AddControllers();

// swagger。デバッグのためにこのアプリで定義されているエンドポイントを一覧する
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// アプリケーションサービス層のDI設定
var basePath = Path.GetFullPath(builder.Environment.ContentRootPath);
OverridedApplicationService.ConfigureServices(builder.Services, basePath);

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
    app.UseStaticFiles();
}

app.MapDefaultControllerRoute();

// Nijoにより自動生成されたクエリ・コマンドをHTTPエンドポイントとして公開する
app.MapOperationEndpoints();

// 存在しないパスへのリクエストは index.html を返すようにし、
// 実際にどの画面が初期表示されるかは React Router のルーティング設定で制御する。
// 開発環境ではViteの開発サーバーがHTMLを配信するため、ここでのフォールバックは不要
if (!app.Environment.IsDevelopment()) {
    app.MapFallbackToFile("index.html");
}

// セキュリティの設定が必要なら追加
// app.UseHttpsRedirection();

app.Run();

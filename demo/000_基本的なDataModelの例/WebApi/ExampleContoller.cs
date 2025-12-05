using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Debugging;

namespace MyApp.WebApi;

/// <summary>
/// 自動生成に頼らず自前でControllerを定義する場合の実装例
/// </summary>
[ApiController]
[Route("/example")]
public class ExampleContoller : ControllerBase {

    // 設定やサービスクラスは DI コンテナ経由で受け取ってください。
    public ExampleContoller(OverridedApplicationConfigure config, OverridedApplicationService app) {
        _config = config;
        _app = app;
    }
    private readonly OverridedApplicationConfigure _config;
    private readonly OverridedApplicationService _app;

    /// <summary>
    /// クライアント側とサーバー側の疎通確認の例。
    /// </summary>
    [HttpGet]
    public IActionResult Index() {

        return Ok($$"""
            ASP.NET Core サーバーとの接続に成功しました。

            実行時設定ファイル(appsettings.json)の内容は以下です。

            {{_config.ToJson(_app.Settings, writeIndented: true)}}
            """);
    }

    /// <summary>
    /// データベースを削除して再作成する。
    /// </summary>
    [HttpPost("destroy-and-recreate-database")]
    public async Task<IActionResult> DestroyAndRecreateDatabase() {
        // データベースを削除して再作成
        await _app.DbContext.Database.EnsureDeletedAsync();
        await _app.DbContext.EnsureCreatedAsyncEx(_app.Settings);

        // ダミーデータの生成
        var generator = new OverridedDummyDataGenerator();
        var descriptor = new DummyDataDbOutput(_app.DbContext);
        await generator.GenerateAsync(descriptor);

        return Ok();
    }

    /// <summary>
    /// テーブルやビューを組み合わせたLINQ式のSQL発行確認
    /// </summary>
    [HttpGet("test-efcore")]
    public async Task<IActionResult> TestEfCore() {
        try {
            var data = await _app.DbContext.社員DbSet
                .Include(x => x.所属部署)
                .ThenInclude(x => x!.事業所)
                .Select(x => new {
                    ID = x.社員ID,
                    氏名 = x.氏名,
                    部署 = x.所属部署 == null ? null : x.所属部署.部署名,
                    事業所 = x.所属部署 != null && x.所属部署.事業所 != null ? x.所属部署.事業所.事業所名 : null,
                })
                .ToArrayAsync();

            return Ok(data);
        } catch (Exception ex) {
            return Problem(ex.ToString());
        }
    }
}

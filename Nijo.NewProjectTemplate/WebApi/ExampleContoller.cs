using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

    [HttpGet]
    public IActionResult Index() {

        return Ok($$"""
            ASP.NET Core サーバーとの接続に成功しました。

            実行時設定ファイル(appsettings.json)の内容は以下です。

            {{_config.ToJson(_app.Settings, writeIndented: true)}}
            """);
    }
}

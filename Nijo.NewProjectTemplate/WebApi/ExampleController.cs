#if DEBUG

using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.WebApi;

/// <summary>
/// 自動生成に頼らず自前でControllerを定義する場合の実装例
/// </summary>
[ApiController]
[Route("api/example")]
[AllowAnonymous]
public class ExampleController : ControllerBase {

    // 設定やサービスクラスは DI コンテナ経由で受け取ってください。
    public ExampleController(OverridedApplicationService app) {
        _app = app;
    }
    private readonly OverridedApplicationService _app;

    /// <summary>
    /// クライアント側とサーバー側の疎通確認の例。
    /// </summary>
    [HttpGet]
    public IActionResult Index() {

        var jsonOptions = new JsonSerializerOptions().EditDefaultJsonSerializerOptions();

        return Ok($$"""
            ASP.NET Core サーバーとの接続に成功しました。

            実行時設定ファイル(appsettings.json)の内容は以下です。

            {{JsonSerializer.Serialize(_app.Settings, jsonOptions)}}
            """);
    }
}

#endif

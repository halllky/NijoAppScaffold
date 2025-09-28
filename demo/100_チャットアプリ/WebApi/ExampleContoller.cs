using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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

        // テスト用アカウントを追加
        await CreateTestAccounts();

        return Ok();
    }

    /// <summary>
    /// テスト用のアカウントを作成する
    /// </summary>
    [HttpPost("create-test-accounts")]
    public async Task<IActionResult> CreateTestAccounts() {
        // テスト用の固定アカウントを追加
        var testAccount = new アカウントDbEntity {
            アカウントID = "test",
            アカウント名 = "テストユーザー",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            CreateUser = "system",
            UpdateUser = "system",
            Version = 1,
        };

        // パスワードをハッシュ化
        testAccount.SetPassword("password");

        // 既存のアカウントがあるかチェック
        var existingTest = await _app.DbContext.アカウントDbSet
            .FirstOrDefaultAsync(a => a.アカウントID == "test");
        if (existingTest == null) {
            _app.DbContext.アカウントDbSet.Add(testAccount);
        }

        // 管理者アカウントも追加
        var adminAccount = new アカウントDbEntity {
            アカウントID = "admin",
            アカウント名 = "管理者",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            CreateUser = "system",
            UpdateUser = "system",
            Version = 1,
        };

        adminAccount.SetPassword("admin123");

        var existingAdmin = await _app.DbContext.アカウントDbSet
            .FirstOrDefaultAsync(a => a.アカウントID == "admin");
        if (existingAdmin == null) {
            _app.DbContext.アカウントDbSet.Add(adminAccount);
        }

        await _app.DbContext.SaveChangesAsync();

        return Ok(new {
            message = "テストアカウントを作成しました",
            accounts = new[] {
                new { id = "test", password = "password", name = "テストユーザー" },
                new { id = "admin", password = "admin123", name = "管理者" }
                         }
        });
    }

    /// <summary>
    /// 現在のログイン状態を確認する
    /// </summary>
    [HttpGet("login-status")]
    public IActionResult GetLoginStatus() {
        if (User.Identity?.IsAuthenticated == true) {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            return Ok(new {
                isLoggedIn = true,
                userId = userId,
                userName = userName
            });
        }

        return Ok(new {
            isLoggedIn = false
        });
    }

    /// <summary>
    /// 認証が必要なテストエンドポイント
    /// </summary>
    [HttpGet("protected")]
    [Authorize]
    public IActionResult ProtectedEndpoint() {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok(new {
            message = "認証済みユーザーのみアクセス可能です",
            userId = userId,
            userName = userName
        });
    }
}

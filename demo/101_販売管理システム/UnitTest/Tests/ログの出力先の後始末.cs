using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyApp.UnitTest;

public class ログの出力先の後始末 {

    [Test]
    [Category("DB接続なし")]
    public void DIコンテナを破棄するとログの出力先が閉じまだ送っていない運用担当への通知を送る() {
        var workDirectory = Path.Combine(Path.GetTempPath(), $"{nameof(ログの出力先の後始末)}_{Guid.NewGuid():N}");
        var mockDirectory = Path.Combine(workDirectory, "通知のモック");
        Directory.CreateDirectory(workDirectory);
        File.WriteAllText(Path.Combine(workDirectory, "appsettings.json"), JsonSerializer.Serialize(new {
            MyApp = new {
                LogDirectory = Path.Combine(workDirectory, "Log"),
                MigrationsScriptFolder = "MigrationsScript",
                CurrentDbProfileName = "DB",
                DbProfiles = new[] { new { Name = "DB", ConnStr = $"Data Source={Path.Combine(workDirectory, "db.sqlite3")}" } },
                BusinessTimeZone = "Asia/Tokyo",
                ErrorNotification = new { UseMock = true, MockOutputDirectory = mockDirectory, DefaultRecipients = new[] { "ops@example.com" }, ThrottleMinutes = 10 },
                商品管理システム = new { UseMock = true },
            },
        }));

        var services = new ServiceCollection();
        OverridedApplicationService.ConfigureServices(services, workDirectory, isDevelopment: true);
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ILogger<ログの出力先の後始末>>().LogError("後始末のテスト");
        provider.Dispose();

        Assert.That(Directory.Exists(mockDirectory) ? Directory.GetFiles(mockDirectory) : [], Has.Length.EqualTo(1));
        Directory.Delete(workDirectory, recursive: true);
    }
}

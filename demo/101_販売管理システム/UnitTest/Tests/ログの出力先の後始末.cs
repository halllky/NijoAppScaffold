using System.Text.Json.Nodes;
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
        // 設定は WebApi のものを使い、ファイルの出力先だけをこのテストのフォルダに差し替える
        var webApiSettings = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.WorkDirectory, "..", "..", "..", "..", "WebApi", "appsettings.json"));
        var settings = JsonNode.Parse(File.ReadAllText(webApiSettings))!;
        var myApp = settings[RuntimeSetting.MY_APP_SECTION]!;
        myApp[nameof(RuntimeSetting.LogDirectory)] = Path.Combine(workDirectory, "Log");
        myApp[nameof(RuntimeSetting.ErrorNotification)]![nameof(RuntimeSetting.ErrorNotification.MockOutputDirectory)] = mockDirectory;
        File.WriteAllText(Path.Combine(workDirectory, "appsettings.json"), settings.ToJsonString());

        var services = new ServiceCollection();
        OverridedApplicationService.ConfigureServices(services, workDirectory, isDevelopment: true);
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ILogger<ログの出力先の後始末>>().LogError("後始末のテスト");
        provider.Dispose();

        Assert.That(Directory.Exists(mockDirectory) ? Directory.GetFiles(mockDirectory) : [], Has.Length.EqualTo(1));
        Directory.Delete(workDirectory, recursive: true);
    }
}

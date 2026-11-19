namespace MyApp.UnitTest;

public class 設定の検証 {

    [Test]
    [Category("DB接続なし")]
    public void 省略できない項目が省略されていたら項目の名前を示して例外になる() {
        var settings = CreateValidSettings();
        settings.BusinessTimeZone = "";
        settings.ErrorNotification.ThrottleMinutes = 0;

        var ex = Assert.Throws<InvalidOperationException>(settings.ThrowIfInvalid);
        Assert.That(ex!.Message, Does.Contain("MyApp:BusinessTimeZone").And.Contain("MyApp:ErrorNotification:ThrottleMinutes"));
    }

    [Test]
    [Category("DB接続なし")]
    public void 省略できない項目がすべてあれば例外にならない() {
        Assert.DoesNotThrow(CreateValidSettings().ThrowIfInvalid);
    }

    private static RuntimeSetting CreateValidSettings() => new() {
        LogDirectory = "Log",
        MigrationsScriptFolder = "MigrationsScript",
        CurrentDbProfileName = "DB",
        BusinessTimeZone = "Asia/Tokyo",
        ErrorNotification = new() {
            UseMock = true,
            MockOutputDirectory = "Mock",
            DefaultRecipients = ["ops@example.com"],
            ThrottleMinutes = 10,
        },
        Job = new() {
            WorkDirectory = "Job",
            MaxConcurrency = 1,
            ShutdownTimeoutSeconds = 30,
        },
        Attachment = new() {
            StorageDirectory = "Attachment",
            MaxFileSizeMegabytes = 10,
            UnreferencedRetentionDays = 90,
        },
    };
}

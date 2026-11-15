namespace MyApp.UnitTest;

public class 業務のタイムゾーン {

    [Test]
    [Category("DB接続なし")]
    [TestCase("Asia/Tokyo", 9)]
    [TestCase("Pacific/Kiritimati", 14)]
    public void 現在時刻はサーバーのタイムゾーンによらず業務のタイムゾーンの時刻になる(string timeZone, int offsetHours) {
        var currentTime = new CurrentTime(new RuntimeSetting { BusinessTimeZone = timeZone });

        Assert.That(currentTime.Value - DateTime.UtcNow, Is.EqualTo(TimeSpan.FromHours(offsetHours)).Within(TimeSpan.FromMinutes(1)));
    }

    [Test]
    [Category("DB接続なし")]
    public void 存在しないタイムゾーンを指定すると例外になる() {
        var settings = new RuntimeSetting { BusinessTimeZone = "存在しないタイムゾーン" };

        Assert.Throws<InvalidOperationException>(() => settings.GetBusinessTimeZone());
    }
}

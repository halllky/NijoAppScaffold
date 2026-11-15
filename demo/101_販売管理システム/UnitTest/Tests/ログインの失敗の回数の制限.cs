using System.Security.Cryptography;
using MyApp.Core.Authorization;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ログインに続けて失敗したユーザーIDは正しいパスワードでもログインできない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(ログインに続けて失敗したユーザーIDは正しいパスワードでもログインできない));
        var salt = RandomNumberGenerator.GetBytes(32);
        await Add従業員Async(scope.App.DbContext, CreateVersion1Hash(PASSWORD, salt, CURRENT_ITERATIONS), salt);

        for (var i = 0; i < LoginFailureHistory.MAX_FAILURES - 1; i++) {
            Assert.That(await TryLoginAsync(scope.App, "wrong-password"), Is.False);
        }
        Assert.That(await TryLoginAsync(scope.App, PASSWORD), Is.True, "規定の回数に達するまでは、正しいパスワードでログインできる");

        for (var i = 0; i < LoginFailureHistory.MAX_FAILURES; i++) {
            Assert.That(await TryLoginAsync(scope.App, "wrong-password"), Is.False);
        }
        Assert.That(await TryLoginAsync(scope.App, PASSWORD), Is.False, "規定の回数に達したら、正しいパスワードでもログインできない");
    }
}

public class ログインの失敗の記録 {

    private static readonly DateTime T0 = new(2026, 1, 1, 9, 0, 0);

    [Test]
    [Category("DB接続なし")]
    public void 期間の中で規定の回数だけ失敗すると一定時間だけログインを受け付けない() {
        var history = new LoginFailureHistory();
        for (var i = 0; i < LoginFailureHistory.MAX_FAILURES; i++) {
            history.RecordFailure("USER", T0.AddMinutes(i));
        }
        var lastFailedAt = T0.AddMinutes(LoginFailureHistory.MAX_FAILURES - 1);

        Assert.That(history.IsBlocked("USER", lastFailedAt + LoginFailureHistory.BlockDuration - TimeSpan.FromSeconds(1)), Is.True);
        Assert.That(history.IsBlocked("USER", lastFailedAt + LoginFailureHistory.BlockDuration), Is.False);
        Assert.That(history.IsBlocked("OTHER_USER", lastFailedAt), Is.False, "ほかのユーザー ID には影響しない");
    }

    [Test]
    [Category("DB接続なし")]
    public void ユーザーIDの前後の空白と大文字小文字の違いは同じユーザーIDとして数える() {
        var history = new LoginFailureHistory();
        string[] userIds = ["user", "USER", " User ", "user ", "uSeR"];
        for (var i = 0; i < LoginFailureHistory.MAX_FAILURES; i++) {
            history.RecordFailure(userIds[i], T0.AddMinutes(i));
        }

        Assert.That(history.IsBlocked("user", T0.AddMinutes(LoginFailureHistory.MAX_FAILURES)), Is.True);
    }

    [Test]
    [Category("DB接続なし")]
    public void 期間より前の失敗は数えない() {
        var history = new LoginFailureHistory();
        history.RecordFailure("USER", T0);
        for (var i = 1; i < LoginFailureHistory.MAX_FAILURES; i++) {
            history.RecordFailure("USER", T0 + LoginFailureHistory.Window + TimeSpan.FromMinutes(i));
        }

        Assert.That(history.IsBlocked("USER", T0 + LoginFailureHistory.Window + TimeSpan.FromMinutes(LoginFailureHistory.MAX_FAILURES)), Is.False);
    }

    [Test]
    [Category("DB接続なし")]
    public void ログインに成功すると失敗の記録は消える() {
        var history = new LoginFailureHistory();
        for (var i = 0; i < LoginFailureHistory.MAX_FAILURES - 1; i++) {
            history.RecordFailure("USER", T0.AddMinutes(i));
        }
        history.Clear("USER");
        history.RecordFailure("USER", T0.AddMinutes(LoginFailureHistory.MAX_FAILURES));

        Assert.That(history.IsBlocked("USER", T0.AddMinutes(LoginFailureHistory.MAX_FAILURES)), Is.False);
    }
}

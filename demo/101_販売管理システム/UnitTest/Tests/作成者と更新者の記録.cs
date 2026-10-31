using Microsoft.EntityFrameworkCore;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 作成者と更新者にログインユーザーが記録される() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(作成者と更新者にログインユーザーが記録される));
        var db = scope.App.DbContext;

        db.従業員DbSet.Add(new 従業員DbEntity { 従業員番号 = "LOGIN_USER", 氏名 = "ログインユーザー", パスワード = [1], SALT = [1], 入荷担当 = false, 販売担当 = false, システム管理者 = false, Version = 0 });
        db.セッションDbSet.Add(new セッションDbEntity {
            セッションキー = TestUtilImpl.SessionKeyProviderInUnitTest.SESSION_KEY,
            ユーザ_従業員番号 = "LOGIN_USER",
            最終ログイン日時 = new DateTime(2026, 1, 1),
            Version = 0,
        });
        await db.SaveChangesAsync();

        var saved = await Save新しい従業員Async(db);

        Assert.That(saved.CreateUser, Is.EqualTo("LOGIN_USER"));
        Assert.That(saved.UpdateUser, Is.EqualTo("LOGIN_USER"));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ログインユーザーがいなければ作成者と更新者に固定値が記録される() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(ログインユーザーがいなければ作成者と更新者に固定値が記録される));

        var saved = await Save新しい従業員Async(scope.App.DbContext);

        Assert.That(saved.CreateUser, Is.EqualTo(AuditInfo.NO_LOGIN_USER));
        Assert.That(saved.UpdateUser, Is.EqualTo(AuditInfo.NO_LOGIN_USER));
    }

    private static async Task<従業員DbEntity> Save新しい従業員Async(OverridedDbContext db) {
        await using var tran = await db.Database.BeginTransactionAsync();

        var command = db.StartCreating従業員();
        command.従業員番号 = "NEW_USER";
        command.氏名 = "新しい従業員";
        command.パスワード = [1];
        command.SALT = [1];
        command.入荷担当 = false;
        command.販売担当 = false;
        command.システム管理者 = false;

        (await db.TrySaveAsync(command.AssertValid())).AssertSucceeded();

        return await db.従業員DbSet.SingleAsync(x => x.従業員番号 == "NEW_USER");
    }
}

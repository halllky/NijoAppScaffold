using Microsoft.EntityFrameworkCore;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 作成者と更新者にログインユーザーが記録される() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(作成者と更新者にログインユーザーが記録される));
        var db = scope.App.DbContext;

        db.従業員DbSet.Add(new 従業員DbEntity { 従業員番号 = "LOGIN_USER", 氏名 = "ログインユーザー", パスワード = [1], SALT = [1], Version = 0 });
        db.セッションDbSet.Add(new セッションDbEntity {
            セッションキー = Core.Authorization.SessionKey.ToStoredValue(TestUtilImpl.SessionKeyProviderInUnitTest.SESSION_KEY),
            ユーザ_従業員番号 = "LOGIN_USER",
            最終操作日時 = DateTime.Now,
            Version = 0,
        });
        await db.SaveChangesAsync();

        var saved = await Save新しい従業員Async(db);

        Assert.That(saved.CreateUser, Is.EqualTo("LOGIN_USER"));
        Assert.That(saved.UpdateUser, Is.EqualTo("LOGIN_USER"));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ログインユーザーがいなければ作成者と更新者に処理名が記録される() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(ログインユーザーがいなければ作成者と更新者に処理名が記録される));

        var saved = await Save新しい従業員Async(scope.App.DbContext);

        // 単体テストでは、処理名はテストケースの名前になる
        Assert.That(saved.CreateUser, Is.EqualTo(nameof(ログインユーザーがいなければ作成者と更新者に処理名が記録される)));
        Assert.That(saved.UpdateUser, Is.EqualTo(nameof(ログインユーザーがいなければ作成者と更新者に処理名が記録される)));
    }

    private static async Task<従業員DbEntity> Save新しい従業員Async(OverridedDbContext db) {
        await using var tran = await db.Database.BeginTransactionAsync();

        var command = db.StartCreating従業員();
        command.従業員番号 = "NEW_USER";
        command.氏名 = "新しい従業員";
        command.パスワード = [1];
        command.SALT = [1];

        (await db.TrySaveAsync(command.AssertValid())).AssertSucceeded();

        return await db.従業員DbSet.SingleAsync(x => x.従業員番号 == "NEW_USER");
    }
}

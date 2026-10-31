namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 処理中のトランザクションで登録されたセッションからログインユーザーを特定できる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(処理中のトランザクションで登録されたセッションからログインユーザーを特定できる));
        var db = scope.App.DbContext;

        await using var tran = await db.Database.BeginTransactionAsync();

        db.従業員DbSet.Add(new 従業員DbEntity { 従業員番号 = "LOGIN_USER", 氏名 = "ログインユーザー", パスワード = [1], SALT = [1], Version = 0 });
        db.セッションDbSet.Add(new セッションDbEntity {
            セッションキー = TestUtilImpl.SessionKeyProviderInUnitTest.SESSION_KEY,
            ユーザ_従業員番号 = "LOGIN_USER",
            最終ログイン日時 = new DateTime(2026, 1, 1),
            Version = 0,
        });
        await db.SaveChangesAsync();

        Assert.That(scope.App.LoginUser?.従業員番号, Is.EqualTo("LOGIN_USER"));
    }
}

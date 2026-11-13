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
            セッションキー = Core.Authorization.SessionKey.ToStoredValue(TestUtilImpl.SessionKeyProviderInUnitTest.SESSION_KEY),
            ユーザ_従業員番号 = "LOGIN_USER",
            最終操作日時 = DateTime.Now,
            Version = 0,
        });
        await db.SaveChangesAsync();

        Assert.That(scope.App.LoginUser?.従業員番号, Is.EqualTo("LOGIN_USER"));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 最後に操作してから有効期限が過ぎたセッションからはログインユーザーを特定しない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(最後に操作してから有効期限が過ぎたセッションからはログインユーザーを特定しない));
        var db = scope.App.DbContext;

        db.従業員DbSet.Add(new 従業員DbEntity { 従業員番号 = "LOGIN_USER", 氏名 = "ログインユーザー", パスワード = [1], SALT = [1], Version = 0 });
        db.セッションDbSet.Add(new セッションDbEntity {
            セッションキー = Core.Authorization.SessionKey.ToStoredValue(TestUtilImpl.SessionKeyProviderInUnitTest.SESSION_KEY),
            ユーザ_従業員番号 = "LOGIN_USER",
            最終操作日時 = DateTime.Now - Core.Authorization.SessionKey.IdleTimeout - TimeSpan.FromMinutes(1),
            Version = 0,
        });
        await db.SaveChangesAsync();

        Assert.That(scope.App.LoginUser, Is.Null);
    }
}

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 読み込んだ時点よりバージョンが進んでいたら楽観排他のエラーとして区別できる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(読み込んだ時点よりバージョンが進んでいたら楽観排他のエラーとして区別できる));
        var db = scope.App.DbContext;
        db.従業員DbSet.Add(new 従業員DbEntity { 従業員番号 = "E1", 氏名 = "氏名", パスワード = [1], SALT = [1], Version = 1 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // 画面が読み込んだのはバージョン 0 の時点で、その後ほかの人がバージョン 1 に更新した
        var command = await db.LoadForEdit従業員Async(new 従業員KeyWithVersion(0) { 従業員番号 = "E1" });
        Assert.That(command!.TryValidate(out _, out _), Is.False);

        Assert.That(db.HasConcurrencyConflict, Is.True);
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ほかのエラーは楽観排他のエラーとして扱わない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(ほかのエラーは楽観排他のエラーとして扱わない));
        var db = scope.App.DbContext;

        var command = db.StartCreating従業員();
        Assert.That(command.TryValidate(out _, out _), Is.False, "必須の項目が空なのでエラーになる");

        Assert.That(db.HasConcurrencyConflict, Is.False);
    }
}

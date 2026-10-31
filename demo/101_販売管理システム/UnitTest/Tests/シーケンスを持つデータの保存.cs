namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task シーケンスが保存後のコマンドに反映され同時に複数件保存できる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(シーケンスが保存後のコマンドに反映され同時に複数件保存できる));
        var db = scope.App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();

        var first = db.StartCreating商品();
        first.外部システム側ID = "EX-1";
        first.商品名 = "商品1";
        var second = db.StartCreating商品();
        second.外部システム側ID = "EX-2";
        second.商品名 = "商品2";

        // キーが保存時に採番されるので、同時に保存してもキー重複とは扱われない
        var saved = (await db.TrySaveAsync([first, second])).AssertSucceeded();

        Assert.Multiple(() => {
            Assert.That(saved.Select(x => x.外部システム側ID), Is.EqualTo(new[] { "EX-1", "EX-2" }));
            Assert.That(saved.Select(x => x.商品SEQ), Is.All.Not.Null);
            Assert.That(saved[0].商品SEQ, Is.Not.EqualTo(saved[1].商品SEQ));
        });
    }
}

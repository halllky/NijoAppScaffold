using Microsoft.EntityFrameworkCore;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 検証後に配列の中身を書き換えても検証時点の値で保存される() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(検証後に配列の中身を書き換えても検証時点の値で保存される));
        var db = scope.App.DbContext;
        await using var tran = await db.Database.BeginTransactionAsync();

        var password = new byte[] { 1, 2, 3 };
        var command = db.StartCreating従業員();
        command.従業員番号 = "E1";
        command.氏名 = "氏名";
        command.パスワード = password;
        command.SALT = [9];

        var validated = command.AssertValid();
        password[0] = 255;
        (await db.TrySaveAsync(validated)).AssertSucceeded();

        var saved = await db.従業員DbSet.SingleAsync(x => x.従業員番号 == "E1");
        Assert.That(saved.パスワード, Is.EqualTo(new byte[] { 1, 2, 3 }));
    }
}

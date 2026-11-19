using System.Text;
using Microsoft.EntityFrameworkCore;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 一時保存したファイルは中身が保存先に置かれ一時保存の状態で記録される() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(一時保存したファイルは中身が保存先に置かれ一時保存の状態で記録される));
        var app = scope.App;

        var messages = new MessageSetter([], new());
        var file = await app.一時保存添付ファイルAsync("C:\\fakepath\\見積書.txt", "text/plain", ToStream("見積の内容"), messages);

        Assert.That(messages.HasError(), Is.False, string.Join(", ", messages.GetAllMessages()));
        Assert.That(file, Is.Not.Null);
        Assert.That(file!.名前, Is.EqualTo("見積書.txt"), "パスが含まれていたらファイル名の部分だけを使う");
        Assert.That(file.サイズ, Is.EqualTo(Encoding.UTF8.GetByteCount("見積の内容")));
        Assert.That(file.識別子, Has.Length.EqualTo(64));

        var saved = await app.DbContext.添付ファイルDbSet.AsNoTracking().SingleAsync();
        Assert.That(saved.識別子, Is.EqualTo(file.識別子));
        Assert.That(saved.状態, Is.EqualTo(添付ファイルの状態.一時保存));
        Assert.That(saved.種類, Is.EqualTo("text/plain"));

        await using var content = await app.Open添付ファイルAsync(file.識別子);
        Assert.That(await new StreamReader(content).ReadToEndAsync(), Is.EqualTo("見積の内容"));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task サイズの上限を超えるファイルは一時保存できず保存先にも残らない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(サイズの上限を超えるファイルは一時保存できず保存先にも残らない));
        var app = scope.App;
        app.Settings.Attachment.MaxFileSizeMegabytes = 1;

        var messages = new MessageSetter([], new());
        var file = await app.一時保存添付ファイルAsync("大きい.bin", null, new MemoryStream(new byte[1024 * 1024 + 1]), messages);

        Assert.That(file, Is.Null);
        Assert.That(messages.GetAllMessages(), Has.Some.Contains("1MB"));
        Assert.That(await app.DbContext.添付ファイルDbSet.AnyAsync(), Is.False);
        Assert.That(Directory.Exists(app.Settings.Attachment.StorageDirectory)
            ? Directory.GetFiles(app.Settings.Attachment.StorageDirectory)
            : [], Is.Empty);
    }

    private static MemoryStream ToStream(string text) => new(Encoding.UTF8.GetBytes(text));
}

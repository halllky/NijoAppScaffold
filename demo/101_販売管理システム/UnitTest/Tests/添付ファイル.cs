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

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 確定したファイルは誰でも一時保存のファイルは本人だけがダウンロードでき参照なしのファイルは誰もダウンロードできない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(確定したファイルは誰でも一時保存のファイルは本人だけがダウンロードでき参照なしのファイルは誰もダウンロードできない));
        var db = scope.App.DbContext;
        await Add従業員Async(db, "USER_A");
        await Add従業員Async(db, "USER_B");

        string 確定, 一時保存, 参照なし;
        await using (var request = await NewRequestAsAsync(scope, "USER_A")) {
            確定 = await 一時保存Async(request.Value, "確定.txt");
            一時保存 = await 一時保存Async(request.Value, "一時保存.txt");
            参照なし = await 一時保存Async(request.Value, "参照なし.txt");
        }
        await db.添付ファイルDbSet.Where(f => f.識別子 == 確定).ExecuteUpdateAsync(x => x.SetProperty(f => f.状態, 添付ファイルの状態.確定));
        await db.添付ファイルDbSet.Where(f => f.識別子 == 参照なし).ExecuteUpdateAsync(x => x.SetProperty(f => f.状態, 添付ファイルの状態.参照なし));

        async Task<string?> DownloadAsAsync(string 従業員番号, string 識別子) {
            await using var request = await NewRequestAsAsync(scope, 従業員番号);
            var file = await request.Value.Findダウンロードできる添付ファイルAsync(識別子);
            if (file == null) return null;
            await using var content = file.中身;
            return $"{file.名前}:{await new StreamReader(content).ReadToEndAsync()}";
        }

        Assert.That(await DownloadAsAsync("USER_B", 確定), Is.EqualTo("確定.txt:確定.txt"));
        Assert.That(await DownloadAsAsync("USER_A", 一時保存), Is.EqualTo("一時保存.txt:一時保存.txt"));
        Assert.That(await DownloadAsAsync("USER_B", 一時保存), Is.Null);
        Assert.That(await DownloadAsAsync("USER_A", 参照なし), Is.Null);
        Assert.That(await DownloadAsAsync("USER_A", "NOT_EXISTS"), Is.Null);
    }

    private static MemoryStream ToStream(string text) => new(Encoding.UTF8.GetBytes(text));

    /// <summary>
    /// テキストファイルを一時保存し、その識別子を返す。
    /// </summary>
    private static async Task<string> 一時保存Async(OverridedApplicationService app, string 名前) {
        var messages = new MessageSetter([], new());
        var file = await app.一時保存添付ファイルAsync(名前, "text/plain", ToStream(名前), messages);
        return file?.識別子 ?? throw new InvalidOperationException(string.Join(", ", messages.GetAllMessages()));
    }
}

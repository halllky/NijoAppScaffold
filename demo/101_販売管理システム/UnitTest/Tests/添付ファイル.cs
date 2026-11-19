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
    public async Task 添付したファイルはデータの保存がコミットされたときに確定する() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(添付したファイルはデータの保存がコミットされたときに確定する));
        var app = scope.App;
        await Add従業員Async(app.DbContext, "USER_A");
        var 取り消される = await 一時保存Async(app, "取り消される.txt");
        var 確定する = await 一時保存Async(app, "確定する.txt");

        await using (var tran = await app.BeginTransactionAsync()) {
            var result = await Save入荷Async(app, "IN-1", 取り消される);
            Assert.That(result.Success, Is.True, string.Join(", ", result.Errors.GetAllMessages()));
            // コミットせずに終わる
        }
        await using (var tran = await app.BeginTransactionAsync()) {
            var result = await Save入荷Async(app, "IN-2", 確定する);
            Assert.That(result.Success, Is.True, string.Join(", ", result.Errors.GetAllMessages()));
            await tran.CommitAsync();
        }

        Assert.That(await Get状態Async(app, 取り消される), Is.EqualTo(添付ファイルの状態.一時保存));
        Assert.That(await Get状態Async(app, 確定する), Is.EqualTo(添付ファイルの状態.確定));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 存在しないファイルと他のユーザーが一時保存したファイルは添付できない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(存在しないファイルと他のユーザーが一時保存したファイルは添付できない));
        await Add従業員Async(scope.App.DbContext, "USER_A");
        var 他人のファイル = await 一時保存Async(scope.App, "他人のファイル.txt"); // ログインユーザーのいない処理として一時保存される

        await using var request = await NewRequestAsAsync(scope, "USER_A");
        await using var tran = await request.Value.BeginTransactionAsync();
        foreach (var 識別子 in new[] { 他人のファイル, "NOT_EXISTS" }) {
            var result = await Save入荷Async(request.Value, $"IN-{識別子}", 識別子);
            Assert.That(result.FailureKind, Is.EqualTo(SaveFailureKind.AttachmentNotAvailable), 識別子);
        }
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 確定しているファイルと参照なしのファイルは他のデータにも添付でき参照なしのファイルは確定に戻る() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(確定しているファイルと参照なしのファイルは他のデータにも添付でき参照なしのファイルは確定に戻る));
        var app = scope.App;
        await Add従業員Async(app.DbContext, "USER_A");
        var 確定済み = await 一時保存Async(app, "確定済み.txt");
        var 参照なし = await 一時保存Async(app, "参照なし.txt");
        await app.DbContext.添付ファイルDbSet.Where(f => f.識別子 == 確定済み).ExecuteUpdateAsync(x => x.SetProperty(f => f.状態, 添付ファイルの状態.確定));
        await app.DbContext.添付ファイルDbSet.Where(f => f.識別子 == 参照なし).ExecuteUpdateAsync(x => x
            .SetProperty(f => f.状態, 添付ファイルの状態.参照なし)
            .SetProperty(f => f.参照なし日時, app.CurrentTime));

        await using var request = await NewRequestAsAsync(scope, "USER_A");
        await using (var tran = await request.Value.BeginTransactionAsync()) {
            var result = await Save入荷Async(request.Value, "IN-1", 確定済み, 参照なし);
            Assert.That(result.Success, Is.True, string.Join(", ", result.Errors.GetAllMessages()));
            await tran.CommitAsync();
        }

        var saved = await app.DbContext.添付ファイルDbSet.AsNoTracking().Where(f => f.識別子 == 参照なし).SingleAsync();
        Assert.That(saved.状態, Is.EqualTo(添付ファイルの状態.確定));
        Assert.That(saved.参照なし日時, Is.Null);
        Assert.That(await Get状態Async(app, 確定済み), Is.EqualTo(添付ファイルの状態.確定));
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

    /// <summary>
    /// 指定したファイルを添付書類にもつ入荷を、USER_A の担当で新規登録する。トランザクションの中で呼ぶ。
    /// </summary>
    private static async Task<SaveResult> Save入荷Async(OverridedApplicationService app, string 入荷ID, params string[] 識別子一覧) {
        var command = app.DbContext.StartCreating入荷();
        command.入荷ID = 入荷ID;
        command.入荷日時 = app.CurrentTime;
        command.担当者 = new() { 従業員番号 = "USER_A" };
        command.入荷の添付書類 = 識別子一覧.Select(識別子 => new 入荷の添付書類CreateCommand {
            添付書類ID = Guid.NewGuid().ToString(),
            ファイル = new() { 識別子 = 識別子 },
            備考 = null,
        }).ToList();
        if (!command.TryValidate(out var validated, out var errors)) {
            throw new InvalidOperationException(string.Join(", ", errors.GetAllMessages()));
        }
        return await app.DbContext.TrySaveAsync(validated);
    }

    private static async Task<添付ファイルの状態?> Get状態Async(OverridedApplicationService app, string 識別子) {
        return await app.DbContext.添付ファイルDbSet.Where(f => f.識別子 == 識別子).Select(f => f.状態).SingleAsync();
    }
}

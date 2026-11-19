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

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task クリーンナップは期限を過ぎた一時保存と参照されなくなってから期間が過ぎたファイルを削除する() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(クリーンナップは期限を過ぎた一時保存と参照されなくなってから期間が過ぎたファイルを削除する));
        var app = scope.App;
        var db = app.DbContext;
        await Add従業員Async(db, "ADMIN", 権限区分.システム管理者);
        await Add従業員Async(db, "USER_A");

        var 古い一時保存 = await 一時保存Async(app, "古い一時保存.txt");
        var 新しい一時保存 = await 一時保存Async(app, "新しい一時保存.txt");
        var 参照されている = await 一時保存Async(app, "参照されている.txt");
        var 参照されなくなった = await 一時保存Async(app, "参照されなくなった.txt");
        var 再び参照された = await 一時保存Async(app, "再び参照された.txt");
        var 古い参照なし = await 一時保存Async(app, "古い参照なし.txt");
        await using (var tran = await app.BeginTransactionAsync()) {
            var result = await Save入荷Async(app, "IN-1", 参照されている, 再び参照された);
            Assert.That(result.Success, Is.True, string.Join(", ", result.Errors.GetAllMessages()));
            await tran.CommitAsync();
        }

        var 一時保存の期限 = app.CurrentTime - OverridedApplicationService.添付ファイルの一時保存の期間;
        var 参照なしの期限 = app.CurrentTime - TimeSpan.FromDays(app.Settings.Attachment.UnreferencedRetentionDays);
        await db.添付ファイルDbSet.Where(f => f.識別子 == 古い一時保存).ExecuteUpdateAsync(x => x.SetProperty(f => f.CreatedAt, 一時保存の期限.AddMinutes(-1)));
        await db.添付ファイルDbSet.Where(f => f.識別子 == 新しい一時保存).ExecuteUpdateAsync(x => x.SetProperty(f => f.CreatedAt, 一時保存の期限.AddMinutes(1)));
        await db.添付ファイルDbSet.Where(f => f.識別子 == 参照されなくなった).ExecuteUpdateAsync(x => x.SetProperty(f => f.状態, 添付ファイルの状態.確定));
        await db.添付ファイルDbSet.Where(f => f.識別子 == 再び参照された).ExecuteUpdateAsync(x => x
            .SetProperty(f => f.状態, 添付ファイルの状態.参照なし)
            .SetProperty(f => f.参照なし日時, 参照なしの期限.AddMinutes(-1)));
        await db.添付ファイルDbSet.Where(f => f.識別子 == 古い参照なし).ExecuteUpdateAsync(x => x
            .SetProperty(f => f.状態, 添付ファイルの状態.参照なし)
            .SetProperty(f => f.参照なし日時, 参照なしの期限.AddMinutes(-1)));

        await using (var request = await NewRequestAsAsync(scope, "ADMIN")) {
            await request.Value.ExecuteクリーンナップAsync(new MessageSetter([], new()), new OperationContext { ValidationOnly = false });
        }

        var remaining = await db.添付ファイルDbSet.AsNoTracking().ToDictionaryAsync(f => f.識別子!, f => f.状態);
        Assert.That(remaining, Is.EquivalentTo(new Dictionary<string, 添付ファイルの状態?> {
            [新しい一時保存] = 添付ファイルの状態.一時保存,
            [参照されている] = 添付ファイルの状態.確定,
            [参照されなくなった] = 添付ファイルの状態.参照なし,
            [再び参照された] = 添付ファイルの状態.確定,
        }));
        var filesInStorage = Directory.GetFiles(app.Settings.Attachment.StorageDirectory).Select(Path.GetFileName);
        Assert.That(filesInStorage, Is.EquivalentTo(remaining.Keys), "削除したファイルは保存先からも消える");
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

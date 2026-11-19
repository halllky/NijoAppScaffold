using Microsoft.EntityFrameworkCore;

namespace MyApp.UnitTest;

/// <summary>
/// 添付ファイルのテストのうち、添付ファイルを参照するデータ（入荷の添付書類）が必要なもの。
/// </summary>
partial class DB接続あり_更新あり {

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

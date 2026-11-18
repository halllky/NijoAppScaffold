using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MyApp.Core.ジョブ;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ジョブは登録した時点では待機で画面から登録すると起動したユーザーが記録される() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(ジョブは登録した時点では待機で画面から登録すると起動したユーザーが記録される));
        await Add従業員Async(scope.App.DbContext, "USER_A");

        var id = await RegisterジョブAsAsync(scope, "USER_A", 何もしないジョブ);

        var job = await Loadジョブ(scope, id);
        Assert.That(job.状態, Is.EqualTo(ジョブの状態.待機));
        Assert.That(job.種類, Is.EqualTo(何もしないジョブ.Name));
        Assert.That(job.起動したユーザー, Is.EqualTo("USER_A"));
        Assert.That(job.開始日時, Is.Null);
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ジョブは登録された順に1件ずつ実行され結果が記録される() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(ジョブは登録された順に1件ずつ実行され結果が記録される));
        var executed = new List<string>();
        var 記録するジョブ = new ジョブの種類<string>("記録するジョブ", (app, 名前, job) => {
            executed.Add(名前);
            job.結果メッセージ = $"{名前}を記録しました。";
            return Task.CompletedTask;
        });
        var queue = CreateジョブキューFor(scope, 記録するジョブ);
        await Add従業員Async(scope.App.DbContext, "USER_A");
        var first = await RegisterジョブAsAsync(scope, "USER_A", 記録するジョブ, "1件目", 登録日時: new DateTime(2026, 1, 1));
        var second = await RegisterジョブAsAsync(scope, "USER_A", 記録するジョブ, "2件目", 登録日時: new DateTime(2026, 1, 2));

        Assert.That(await queue.Run次のジョブAsync(CancellationToken.None), Is.True);
        Assert.That(executed, Is.EqualTo(new[] { "1件目" }));
        Assert.That(await queue.Run次のジョブAsync(CancellationToken.None), Is.True);
        Assert.That(await queue.Run次のジョブAsync(CancellationToken.None), Is.False, "待機中のジョブが無ければ何もしない");
        Assert.That(executed, Is.EqualTo(new[] { "1件目", "2件目" }));

        var job = await Loadジョブ(scope, first);
        Assert.That(job.状態, Is.EqualTo(ジョブの状態.正常終了));
        Assert.That(job.結果メッセージ, Is.EqualTo("1件目を記録しました。"));
        Assert.That(job.開始日時, Is.Not.Null);
        Assert.That(job.終了日時, Is.Not.Null);
        Assert.That(job.実行したサーバー, Is.EqualTo(Environment.MachineName));
        Assert.That(job.UpdateUser, Is.EqualTo("USER_A"), "画面から起動したジョブの更新者は、起動したユーザー");
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 実行中のジョブは二重に取り出されない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(実行中のジョブは二重に取り出されない));
        ジョブキュー? queue = null;
        bool? 実行中に別のジョブを取り出せたか = null;
        var 入れ子のジョブ = new ジョブの種類("入れ子のジョブ", async (app, job) => {
            実行中に別のジョブを取り出せたか = await queue!.Run次のジョブAsync(CancellationToken.None);
        });
        queue = CreateジョブキューFor(scope, 入れ子のジョブ);
        await RegisterジョブAsAsync(scope, null, 入れ子のジョブ);

        await queue.Run次のジョブAsync(CancellationToken.None);

        Assert.That(実行中に別のジョブを取り出せたか, Is.False);
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ジョブの本体には作業フォルダとスプールのフォルダが作られる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(ジョブの本体には作業フォルダとスプールのフォルダが作られる));
        string? spool = null;
        var ファイルを作るジョブ = new ジョブの種類("ファイルを作るジョブ", async (app, job) => {
            spool = job.スプールフォルダ;
            await File.WriteAllTextAsync(Path.Combine(job.スプールフォルダ, "結果.csv"), "a,b");
        });
        var queue = CreateジョブキューFor(scope, ファイルを作るジョブ);
        var id = await RegisterジョブAsAsync(scope, null, ファイルを作るジョブ);

        await queue.Run次のジョブAsync(CancellationToken.None);

        Assert.That(spool, Does.StartWith(scope.App.Settings.Job.Get作業フォルダ(id)));
        Assert.That(File.Exists(Path.Combine(spool!, "結果.csv")), Is.True);
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ジョブの本体が例外を投げたら異常終了になる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(ジョブの本体が例外を投げたら異常終了になる));
        var 失敗するジョブ = new ジョブの種類<string?>("失敗するジョブ", (app, 結果メッセージ, job) => {
            job.結果メッセージ = 結果メッセージ;
            throw new InvalidOperationException("失敗");
        });
        var queue = CreateジョブキューFor(scope, 失敗するジョブ);
        var 説明あり = await RegisterジョブAsAsync(scope, null, 失敗するジョブ, "取り込めないデータがありました。");
        var 説明なし = await RegisterジョブAsAsync(scope, null, 失敗するジョブ, null);

        await queue.Run次のジョブAsync(CancellationToken.None);
        await queue.Run次のジョブAsync(CancellationToken.None);

        var jobs = await scope.App.DbContext.ジョブDbSet.AsNoTracking().ToDictionaryAsync(j => j.ジョブID!);
        Assert.That(jobs.Values.Select(j => j.状態), Is.All.EqualTo(ジョブの状態.異常終了));
        Assert.That(jobs[説明あり].結果メッセージ, Is.EqualTo("取り込めないデータがありました。"));
        Assert.That(jobs[説明なし].結果メッセージ, Is.EqualTo("エラーが発生したため、異常終了しました。"));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task サーバーの停止の合図で中断したジョブは異常終了になる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(サーバーの停止の合図で中断したジョブは異常終了になる));
        using var stopping = new CancellationTokenSource();
        var 長いジョブ = new ジョブの種類("長いジョブ", async (app, job) => {
            stopping.Cancel();
            await Task.Delay(Timeout.Infinite, job.停止の合図);
        });
        var queue = CreateジョブキューFor(scope, 長いジョブ);
        var id = await RegisterジョブAsAsync(scope, null, 長いジョブ);

        await queue.Run次のジョブAsync(stopping.Token);

        var job = await Loadジョブ(scope, id);
        Assert.That(job.状態, Is.EqualTo(ジョブの状態.異常終了));
        Assert.That(job.結果メッセージ, Is.EqualTo("サーバーの停止により、中断しました。"));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 画面から起動したジョブが終わったら起動したユーザーに通知する() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(画面から起動したジョブが終わったら起動したユーザーに通知する));
        var 失敗するジョブ = new ジョブの種類("失敗するジョブ", (app, job) => throw new InvalidOperationException());
        var queue = CreateジョブキューFor(scope, 何もしないジョブ, 失敗するジョブ);
        await Add従業員Async(scope.App.DbContext, "USER_A");
        await RegisterジョブAsAsync(scope, "USER_A", 何もしないジョブ, 登録日時: new DateTime(2026, 1, 1));
        await RegisterジョブAsAsync(scope, "USER_A", 失敗するジョブ, 登録日時: new DateTime(2026, 1, 2));
        await RegisterジョブAsAsync(scope, null, 何もしないジョブ, 登録日時: new DateTime(2026, 1, 3));

        while (await queue.Run次のジョブAsync(CancellationToken.None)) { }

        var notifications = await scope.App.DbContext.通知DbSet.AsNoTracking().OrderBy(n => n.本文).ToListAsync();
        Assert.That(notifications.Select(n => n.宛先), Is.All.EqualTo("USER_A"));
        Assert.That(notifications.Select(n => n.本文), Is.EquivalentTo(new[] {
            "ジョブ「何もしないジョブ」が正常終了しました。",
            "ジョブ「失敗するジョブ」が異常終了しました。",
        }));
        Assert.That(notifications.Select(n => n.リンク先), Is.All.EqualTo(テンプレート.画面のURL.ジョブ()));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task このサーバーで実行中のまま残っているジョブは起動のときに中断として異常終了にする() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(このサーバーで実行中のまま残っているジョブは起動のときに中断として異常終了にする));
        var queue = CreateジョブキューFor(scope, 何もしないジョブ);
        var db = scope.App.DbContext;
        await Add従業員Async(db, "USER_A");
        db.ジョブDbSet.AddRange(
            Newジョブ("このサーバーで実行中", ジョブの状態.実行中, Environment.MachineName, 起動したユーザー: "USER_A"),
            Newジョブ("他のサーバーで実行中", ジョブの状態.実行中, "OTHER_SERVER"),
            Newジョブ("待機", ジョブの状態.待機, null));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await queue.Abort中断されたジョブAsync();

        var jobs = await db.ジョブDbSet.AsNoTracking().ToDictionaryAsync(j => j.ジョブID!, j => j.状態);
        Assert.That(jobs["このサーバーで実行中"], Is.EqualTo(ジョブの状態.異常終了));
        Assert.That(jobs["他のサーバーで実行中"], Is.EqualTo(ジョブの状態.実行中));
        Assert.That(jobs["待機"], Is.EqualTo(ジョブの状態.待機), "中断したジョブを自動で実行し直すことはない");
        Assert.That(await db.通知DbSet.CountAsync(n => n.宛先 == "USER_A"), Is.EqualTo(1));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 定期的なジョブは予定の時刻を過ぎたら起動したユーザーなしで登録される() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(定期的なジョブは予定の時刻を過ぎたら起動したユーザーなしで登録される));
        var 毎分のジョブ = new ジョブの種類("毎分のジョブ", (app, job) => Task.CompletedTask, ジョブの予定.間隔(TimeSpan.FromMinutes(1)));
        var queue = CreateジョブキューFor(scope, 毎分のジョブ, 何もしないジョブ);

        var now = await queue.Register予定のジョブAsync(null);
        Assert.That(await scope.App.DbContext.ジョブDbSet.CountAsync(), Is.EqualTo(0), "サーバーの起動の前の予定は登録しない");

        await queue.Register予定のジョブAsync(now - TimeSpan.FromMinutes(2));

        var registered = await scope.App.DbContext.ジョブDbSet.AsNoTracking().ToListAsync();
        Assert.That(registered.Select(j => j.種類), Is.EqualTo(new[] { "毎分のジョブ" }), "予定の時刻を何度過ぎていても、1回だけ登録する");
        Assert.That(registered[0].起動したユーザー, Is.Null);
        Assert.That(registered[0].CreateUser, Is.EqualTo("毎分のジョブ"), "定期的なジョブの作成者は、ジョブの種類");
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public void ジョブの種類の名前は重複できない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(ジョブの種類の名前は重複できない));

        Assert.Throws<InvalidOperationException>(() => CreateジョブキューFor(scope, 何もしないジョブ, new ジョブの種類(何もしないジョブ.Name, (app, job) => Task.CompletedTask)));
        Assert.DoesNotThrow(() => ジョブキュー.Create(scope.ServiceProvider), "アプリケーションに定義された種類の名前は重複していない");
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task クリーンナップはログインユーザーのいないジョブとして実行できる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(クリーンナップはログインユーザーのいないジョブとして実行できる));
        var queue = CreateジョブキューFor(scope, OverridedApplicationService.クリーンナップのジョブ);
        var id = await RegisterジョブAsAsync(scope, null, OverridedApplicationService.クリーンナップのジョブ);

        await queue.Run次のジョブAsync(CancellationToken.None);

        var job = await Loadジョブ(scope, id);
        Assert.That(job.状態, Is.EqualTo(ジョブの状態.正常終了), job.結果メッセージ);
        Assert.That(OverridedApplicationService.クリーンナップのジョブ.予定, Is.Not.Null, "クリーンナップは定期的に実行する");
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task クリーンナップは保存期間を過ぎた終わったジョブの記録と作業フォルダを削除する() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(クリーンナップは保存期間を過ぎた終わったジョブの記録と作業フォルダを削除する));
        var app = scope.App;
        var db = app.DbContext;
        var expired = app.CurrentTime - OverridedApplicationService.終わったジョブの保存期間 - TimeSpan.FromMinutes(1);
        var recent = app.CurrentTime - OverridedApplicationService.終わったジョブの保存期間 + TimeSpan.FromMinutes(1);
        await Add従業員Async(db, "ADMIN", 権限区分.システム管理者);
        db.ジョブDbSet.AddRange(
            Newジョブ("保存期間を過ぎた正常終了", ジョブの状態.正常終了, Environment.MachineName, 終了日時: expired),
            Newジョブ("保存期間を過ぎた異常終了", ジョブの状態.異常終了, Environment.MachineName, 終了日時: expired),
            Newジョブ("保存期間内の正常終了", ジョブの状態.正常終了, Environment.MachineName, 終了日時: recent),
            Newジョブ("古い待機", ジョブの状態.待機, null));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        foreach (var id in new[] { "保存期間を過ぎた正常終了", "保存期間内の正常終了", "記録の無いジョブ" }) {
            Directory.CreateDirectory(Path.Combine(app.Settings.Job.Get作業フォルダ(id), "spool"));
        }

        await using (var request = await NewRequestAsAsync(scope, "ADMIN")) {
            await request.Value.ExecuteクリーンナップAsync(new MessageSetter([], new()), new OperationContext { ValidationOnly = false });
        }

        var remaining = await db.ジョブDbSet.Select(j => j.ジョブID).ToListAsync();
        Assert.That(remaining, Is.EquivalentTo(new[] { "保存期間内の正常終了", "古い待機" }));
        var folders = Directory.GetDirectories(app.Settings.Job.WorkDirectory).Select(Path.GetFileName);
        Assert.That(folders, Is.EquivalentTo(new[] { "保存期間内の正常終了" }));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ジョブの一覧は自分が起動したジョブだけを返しシステム管理者には全員のジョブを返す() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(ジョブの一覧は自分が起動したジョブだけを返しシステム管理者には全員のジョブを返す));
        var db = scope.App.DbContext;
        await Add従業員Async(db, "ADMIN", 権限区分.システム管理者);
        await Add従業員Async(db, "USER_A");
        db.ジョブDbSet.AddRange(
            Newジョブ("USER_Aのジョブ", ジョブの状態.待機, null, 起動したユーザー: "USER_A"),
            Newジョブ("USER_Bのジョブ", ジョブの状態.待機, null, 起動したユーザー: "USER_B"),
            Newジョブ("定期的なジョブ", ジョブの状態.待機, null));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.That(await LoadジョブIDsAsync(scope, "USER_A"), Is.EquivalentTo(new[] { "USER_Aのジョブ" }));
        Assert.That(await LoadジョブIDsAsync(scope, "ADMIN"), Is.EquivalentTo(new[] { "USER_Aのジョブ", "USER_Bのジョブ", "定期的なジョブ" }));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task スプールのファイルは起動したユーザーとシステム管理者だけがダウンロードできる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(スプールのファイルは起動したユーザーとシステム管理者だけがダウンロードできる));
        var ファイルを作るジョブ = new ジョブの種類("ファイルを作るジョブ", (app, job) => File.WriteAllTextAsync(Path.Combine(job.スプールフォルダ, "結果.csv"), "a,b"));
        var queue = CreateジョブキューFor(scope, ファイルを作るジョブ);
        await Add従業員Async(scope.App.DbContext, "ADMIN", 権限区分.システム管理者);
        await Add従業員Async(scope.App.DbContext, "USER_A");
        await Add従業員Async(scope.App.DbContext, "USER_B");
        var id = await RegisterジョブAsAsync(scope, "USER_A", ファイルを作るジョブ);
        await queue.Run次のジョブAsync(CancellationToken.None);

        Assert.That(await FindスプールのファイルAsAsync(scope, "USER_A", id, "結果.csv"), Is.Not.Null);
        Assert.That(await FindスプールのファイルAsAsync(scope, "ADMIN", id, "結果.csv"), Is.Not.Null);
        Assert.That(await FindスプールのファイルAsAsync(scope, "USER_B", id, "結果.csv"), Is.Null, "他のユーザーが起動したジョブのファイルはダウンロードできない");
        Assert.That(await FindスプールのファイルAsAsync(scope, "USER_A", id, "存在しない.csv"), Is.Null);
        Assert.That(await FindスプールのファイルAsAsync(scope, "USER_A", id, "../../appsettings.json"), Is.Null, "スプールの外のファイルは指定できない");

        await using var request = await NewRequestAsAsync(scope, "USER_A");
        var loaded = await request.Value.LoadAsync(new ジョブ一覧SearchCondition(), new ジョブ一覧SearchConditionMessages([], new()), new OperationContext { ValidationOnly = false });
        Assert.That(loaded!.CurrentPageItems.Single().スプールのファイル.Select(f => f.ファイル名), Is.EqualTo(new[] { "結果.csv" }), "一覧にはスプールのファイルの名前が載る");
    }

    private static async Task<string[]> LoadジョブIDsAsync(TestScopeImpl scope, string ログインユーザー) {
        await using var request = await NewRequestAsAsync(scope, ログインユーザー);
        var loaded = await request.Value.LoadAsync(new ジョブ一覧SearchCondition(), new ジョブ一覧SearchConditionMessages([], new()), new OperationContext { ValidationOnly = false });
        return loaded!.CurrentPageItems.Select(j => j.ジョブID!).ToArray();
    }

    private static async Task<string?> FindスプールのファイルAsAsync(TestScopeImpl scope, string ログインユーザー, string ジョブID, string ファイル名) {
        await using var request = await NewRequestAsAsync(scope, ログインユーザー);
        return await request.Value.FindスプールのファイルAsync(ジョブID, ファイル名);
    }

    private static readonly ジョブの種類 何もしないジョブ = new("何もしないジョブ", (app, job) => Task.CompletedTask);

    /// <summary>
    /// テスト用のジョブの種類だけを扱うジョブキューを作る。
    /// </summary>
    private static ジョブキュー CreateジョブキューFor(TestScopeImpl scope, params ジョブの種類[] 種類) {
        return new(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>(), NullLogger<ジョブキュー>.Instance, 種類);
    }

    /// <summary>
    /// 指定した従業員がログインしている状態で、ジョブを登録する。従業員が null ならログインしていない状態で登録する。
    /// 登録日時を指定した場合は、登録後にその日時で上書きする。
    /// </summary>
    private static Task<string> RegisterジョブAsAsync(TestScopeImpl scope, string? 従業員番号, ジョブの種類 種類, DateTime? 登録日時 = null) {
        return RegisterジョブAsAsync(scope, 従業員番号, app => app.RegisterジョブAsync(種類), 登録日時);
    }

    /// <inheritdoc cref="RegisterジョブAsAsync(TestScopeImpl, string?, ジョブの種類, DateTime?)"/>
    private static Task<string> RegisterジョブAsAsync<TArgument>(TestScopeImpl scope, string? 従業員番号, ジョブの種類<TArgument> 種類, TArgument 引数, DateTime? 登録日時 = null) {
        return RegisterジョブAsAsync(scope, 従業員番号, app => app.RegisterジョブAsync(種類, 引数), 登録日時);
    }

    private static async Task<string> RegisterジョブAsAsync(TestScopeImpl scope, string? 従業員番号, Func<OverridedApplicationService, Task<string>> registerAsync, DateTime? 登録日時) {
        if (従業員番号 == null) {
            // 前に登録したときのログインが残っていれば、ログインしていない状態に戻す
            await scope.App.DbContext.セッションDbSet.ExecuteDeleteAsync();
        }
        await using var request = 従業員番号 == null ? NewRequest(scope) : await NewRequestAsAsync(scope, 従業員番号);
        string id;
        await using (var tran = await request.Value.BeginTransactionAsync()) {
            id = await registerAsync(request.Value);
            await tran.CommitAsync();
        }
        if (登録日時 != null) {
            await request.Value.DbContext.ジョブDbSet
                .Where(j => j.ジョブID == id)
                .ExecuteUpdateAsync(s => s.SetProperty(j => j.CreatedAt, 登録日時));
        }
        return id;
    }

    private static async Task<ジョブDbEntity> Loadジョブ(TestScopeImpl scope, string ジョブID) {
        return await scope.App.DbContext.ジョブDbSet.AsNoTracking().SingleAsync(j => j.ジョブID == ジョブID);
    }

    /// <summary>
    /// テストの前提となるジョブ。ジョブIDには、どのジョブかを識別するための名前を入れる。
    /// </summary>
    private static ジョブDbEntity Newジョブ(string ジョブID, ジョブの状態 状態, string? 実行したサーバー, string? 起動したユーザー = null, DateTime? 終了日時 = null) => new() {
        ジョブID = ジョブID,
        種類 = 何もしないジョブ.Name,
        状態 = 状態,
        実行したサーバー = 実行したサーバー,
        起動したユーザー = 起動したユーザー,
        終了日時 = 終了日時,
        Version = 0,
    };
}

public class 定期的なジョブの予定 {

    [Test]
    [Category("DB接続なし")]
    public void 毎日の予定は前回確かめた日時より後で今以前に時刻があるかで決まる() {
        var 予定 = ジョブの予定.毎日(new TimeOnly(3, 0));

        Assert.That(予定.Has予定の時刻(new(2026, 1, 1, 2, 59, 0), new(2026, 1, 1, 3, 0, 0)), Is.True);
        Assert.That(予定.Has予定の時刻(new(2026, 1, 1, 3, 0, 0), new(2026, 1, 1, 3, 1, 0)), Is.False, "前回確かめた日時ちょうどの予定は、前回の確認で済んでいる");
        Assert.That(予定.Has予定の時刻(new(2026, 1, 1, 4, 0, 0), new(2026, 1, 2, 2, 59, 0)), Is.False);
        Assert.That(予定.Has予定の時刻(new(2026, 1, 1, 23, 59, 0), new(2026, 1, 2, 3, 0, 0)), Is.True, "日をまたいでも数える");
    }

    [Test]
    [Category("DB接続なし")]
    public void 間隔の予定は毎日0時を起点に数える() {
        var 予定 = ジョブの予定.間隔(TimeSpan.FromHours(7));

        Assert.That(予定.Has予定の時刻(new(2026, 1, 1, 13, 0, 0), new(2026, 1, 1, 14, 0, 0)), Is.True);
        Assert.That(予定.Has予定の時刻(new(2026, 1, 1, 14, 0, 0), new(2026, 1, 1, 20, 59, 0)), Is.False);
        Assert.That(予定.Has予定の時刻(new(2026, 1, 1, 22, 0, 0), new(2026, 1, 2, 0, 0, 0)), Is.True, "21時の次は翌日の0時");
        Assert.Throws<ArgumentOutOfRangeException>(() => ジョブの予定.間隔(TimeSpan.FromSeconds(30)));
    }
}

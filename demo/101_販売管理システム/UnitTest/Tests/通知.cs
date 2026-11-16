using Microsoft.EntityFrameworkCore;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 通知は業務のデータと一緒に確定し取り消されれば残らない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(通知は業務のデータと一緒に確定し取り消されれば残らない));
        var app = scope.App;

        await using (var tran = await app.BeginTransactionAsync()) {
            await app.Create通知Async(["USER_A"], "取り消される通知");
            // コミットせずに終わる
        }
        await using (var tran = await app.BeginTransactionAsync()) {
            await app.Create通知Async(["USER_A"], "確定する通知", "/uriage/1");
            await tran.CommitAsync();
        }

        var saved = await app.DbContext.通知DbSet.AsNoTracking().ToListAsync();
        Assert.That(saved.Select(n => n.本文), Is.EqualTo(new[] { "確定する通知" }));
        Assert.That(saved[0].宛先, Is.EqualTo("USER_A"));
        Assert.That(saved[0].リンク先, Is.EqualTo("/uriage/1"));
        Assert.That(saved[0].既読日時, Is.Null);
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 通知は宛先1人につき1件作られる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(通知は宛先1人につき1件作られる));
        var app = scope.App;

        await using (var tran = await app.BeginTransactionAsync()) {
            await app.Create通知Async(["USER_A", "USER_B", "USER_A"], "本文");
            await tran.CommitAsync();
        }

        var recipients = await app.DbContext.通知DbSet.Select(n => n.宛先).ToListAsync();
        Assert.That(recipients, Is.EquivalentTo(new[] { "USER_A", "USER_B" }));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public void 通知はトランザクションの外では作れない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(通知はトランザクションの外では作れない));

        Assert.ThrowsAsync<InvalidOperationException>(() => scope.App.Create通知Async(["USER_A"], "本文"));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 通知のリンク先はアプリケーションの中のパスに限る() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(通知のリンク先はアプリケーションの中のパスに限る));
        var app = scope.App;

        await using var tran = await app.BeginTransactionAsync();
        foreach (var link in new[] { "https://example.com/", "//example.com/", "/\\example.com/", "uriage/1" }) {
            Assert.ThrowsAsync<ArgumentException>(() => app.Create通知Async(["USER_A"], "本文", link), link);
        }
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task クリーンナップは保存期間を過ぎた既読の通知と宛先のいない通知を削除する() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(クリーンナップは保存期間を過ぎた既読の通知と宛先のいない通知を削除する));
        var app = scope.App;
        var db = app.DbContext;
        var expired = app.CurrentTime - OverridedApplicationService.既読の通知の保存期間 - TimeSpan.FromMinutes(1);
        var recent = app.CurrentTime - OverridedApplicationService.既読の通知の保存期間 + TimeSpan.FromMinutes(1);

        await Add従業員Async(db, "ADMIN", 権限区分.システム管理者);
        await Add従業員Async(db, "USER_A");
        await Add通知Async(db,
            New通知("保存期間を過ぎた既読", "USER_A", 既読日時: expired),
            New通知("保存期間内の既読", "USER_A", 既読日時: recent),
            New通知("古い未読", "USER_A", 既読日時: null),
            New通知("宛先のいない未読", "DELETED_USER", 既読日時: null));

        await using (var request = await NewRequestAsAsync(scope, "ADMIN")) {
            await request.Value.ExecuteクリーンナップAsync(new MessageSetter([], new()), new OperationContext { ValidationOnly = false });
        }

        var remaining = await db.通知DbSet.Select(n => n.通知ID).ToListAsync();
        Assert.That(remaining, Is.EquivalentTo(new[] { "保存期間内の既読", "古い未読" }));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 通知一覧はログインユーザー宛ての通知だけを新しい順に返す() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(通知一覧はログインユーザー宛ての通知だけを新しい順に返す));
        await Add従業員Async(scope.App.DbContext, "USER_A");
        await Add通知Async(scope.App.DbContext,
            New通知("古い自分宛て", "USER_A", 既読日時: null, 作成日時: new DateTime(2026, 1, 1)),
            New通知("新しい自分宛て", "USER_A", 既読日時: null, 作成日時: new DateTime(2026, 1, 2)),
            New通知("他人宛て", "USER_B", 既読日時: null, 作成日時: new DateTime(2026, 1, 3)));

        await using var request = await NewRequestAsAsync(scope, "USER_A");
        var loaded = await request.Value.LoadAsync(new 通知一覧SearchCondition(), new 通知一覧SearchConditionMessages([], new()), new OperationContext { ValidationOnly = false });

        Assert.That(loaded!.CurrentPageItems.Select(n => n.通知ID), Is.EqualTo(new[] { "新しい自分宛て", "古い自分宛て" }));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 通知は既読と未読を切り替えられ一覧を見ただけでは既読にならない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(通知は既読と未読を切り替えられ一覧を見ただけでは既読にならない));
        await Add従業員Async(scope.App.DbContext, "USER_A");
        await Add通知Async(scope.App.DbContext, New通知("通知", "USER_A", 既読日時: null));

        await using (var request = await NewRequestAsAsync(scope, "USER_A")) {
            await request.Value.LoadAsync(new 通知一覧SearchCondition(), new 通知一覧SearchConditionMessages([], new()), new OperationContext { ValidationOnly = false });
        }
        Assert.That(await Load既読日時Async(scope, "通知"), Is.Null);

        await Execute通知既読設定Async(scope, "USER_A", "通知", 既読: true);
        Assert.That(await Load既読日時Async(scope, "通知"), Is.Not.Null);

        await Execute通知既読設定Async(scope, "USER_A", "通知", 既読: false);
        Assert.That(await Load既読日時Async(scope, "通知"), Is.Null);
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 他のユーザー宛ての通知は既読にできない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(他のユーザー宛ての通知は既読にできない));
        await Add従業員Async(scope.App.DbContext, "USER_A");
        await Add通知Async(scope.App.DbContext, New通知("他人宛て", "USER_B", 既読日時: null));

        var messages = await Execute通知既読設定Async(scope, "USER_A", "他人宛て", 既読: true);

        Assert.That(messages.HasError(), Is.True);
        Assert.That(await Load既読日時Async(scope, "他人宛て"), Is.Null);
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task すべて既読にすると自分宛ての未読の通知だけが既読になり未読の件数が0になる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(すべて既読にすると自分宛ての未読の通知だけが既読になり未読の件数が0になる));
        await Add従業員Async(scope.App.DbContext, "USER_A");
        await Add通知Async(scope.App.DbContext,
            New通知("自分宛て1", "USER_A", 既読日時: null),
            New通知("自分宛て2", "USER_A", 既読日時: null),
            New通知("他人宛て", "USER_B", 既読日時: null));

        await using (var request = await NewRequestAsAsync(scope, "USER_A")) {
            var before = await request.Value.Execute未読通知件数Async(new MessageSetter([], new()), new OperationContext { ValidationOnly = false });
            Assert.That(before!.件数, Is.EqualTo(2));
        }
        await using (var request = await NewRequestAsAsync(scope, "USER_A")) {
            var messages = new MessageSetter([], new());
            var context = new OperationContext { ValidationOnly = false };
            await request.Value.Execute通知すべて既読Async(messages, context);
            Assert.That(messages.HasError(), Is.False, TestFailureMessage.Build("すべて既読にできませんでした。", context, messages));
        }
        await using (var request = await NewRequestAsAsync(scope, "USER_A")) {
            var after = await request.Value.Execute未読通知件数Async(new MessageSetter([], new()), new OperationContext { ValidationOnly = false });
            Assert.That(after!.件数, Is.EqualTo(0));
        }
        Assert.That(await Load既読日時Async(scope, "他人宛て"), Is.Null);
    }

    private static async Task<通知既読設定ParameterMessages> Execute通知既読設定Async(TestScopeImpl scope, string ログインユーザー, string 通知ID, bool 既読) {
        await using var request = await NewRequestAsAsync(scope, ログインユーザー);
        var messages = new 通知既読設定ParameterMessages([], new());
        await request.Value.Execute通知既読設定Async(new() { 通知ID = 通知ID, 既読 = 既読 }, messages, new OperationContext { ValidationOnly = false });
        return messages;
    }

    private static async Task<DateTime?> Load既読日時Async(TestScopeImpl scope, string 通知ID) {
        var notification = await scope.App.DbContext.通知DbSet.AsNoTracking().SingleAsync(n => n.通知ID == 通知ID);
        return notification.既読日時;
    }

    private static async Task Add通知Async(OverridedDbContext db, params 通知DbEntity[] notifications) {
        db.通知DbSet.AddRange(notifications);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    /// <summary>
    /// 通知の宛先やログインユーザーにする従業員を登録する。
    /// </summary>
    private static async Task Add従業員Async(OverridedDbContext db, string 従業員番号, params 権限区分[] 権限) {
        db.従業員DbSet.Add(new 従業員DbEntity {
            従業員番号 = 従業員番号,
            氏名 = 従業員番号,
            パスワード = [1],
            SALT = [1],
            Version = 0,
            従業員の権限 = 権限.Select(x => new 従業員の権限DbEntity { 権限区分 = x }).ToList(),
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    /// <summary>
    /// 指定した従業員がログインしている状態の、同じ DB に対する新しいリクエストを作る。
    /// </summary>
    private static async Task<ScopedApp> NewRequestAsAsync(TestScopeImpl scope, string 従業員番号) {
        var db = scope.App.DbContext;
        db.ChangeTracker.Clear(); // それまでの処理（ダミーデータの作成など）が追跡しているセッションと重ならないようにする
        var sessionKey = Core.Authorization.SessionKey.ToStoredValue(TestUtilImpl.SessionKeyProviderInUnitTest.SESSION_KEY);
        await db.セッションDbSet.Where(s => s.セッションキー == sessionKey).ExecuteDeleteAsync();
        db.セッションDbSet.Add(new セッションDbEntity {
            セッションキー = sessionKey,
            ユーザ_従業員番号 = 従業員番号,
            最終操作日時 = scope.App.CurrentTime,
            Version = 0,
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return NewRequest(scope);
    }

    /// <summary>
    /// テストの前提となる通知。通知IDには、どの通知かを見分けるための名前を入れる。
    /// </summary>
    private static 通知DbEntity New通知(string 通知ID, string 宛先, DateTime? 既読日時, DateTime? 作成日時 = null) => new() {
        通知ID = 通知ID,
        宛先 = 宛先,
        本文 = 通知ID,
        既読日時 = 既読日時,
        CreatedAt = 作成日時,
        Version = 0,
    };
}

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
        db.通知DbSet.AddRange(
            New通知("保存期間を過ぎた既読", "USER_A", 既読日時: expired),
            New通知("保存期間内の既読", "USER_A", 既読日時: recent),
            New通知("古い未読", "USER_A", 既読日時: null),
            New通知("宛先のいない未読", "DELETED_USER", 既読日時: null));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await using (var request = await NewRequestAsAsync(scope, "ADMIN")) {
            await request.Value.ExecuteクリーンナップAsync(new MessageSetter([], new()), new OperationContext { ValidationOnly = false });
        }

        var remaining = await db.通知DbSet.Select(n => n.通知ID).ToListAsync();
        Assert.That(remaining, Is.EquivalentTo(new[] { "保存期間内の既読", "古い未読" }));
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
    private static 通知DbEntity New通知(string 通知ID, string 宛先, DateTime? 既読日時) => new() {
        通知ID = 通知ID,
        宛先 = 宛先,
        本文 = 通知ID,
        既読日時 = 既読日時,
        Version = 0,
    };
}

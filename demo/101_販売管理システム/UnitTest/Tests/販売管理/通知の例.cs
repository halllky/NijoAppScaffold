using Microsoft.EntityFrameworkCore;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 担当者以外が売上を修正したら担当者に通知する() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(担当者以外が売上を修正したら担当者に通知する));
        await GenerateダミーデータWithout通知Async(scope);
        var sales = await scope.App.DbContext.売上DbSet.AsNoTracking().FirstAsync();

        // ダミーデータの売上の担当者はシステム管理者。販売担当の user003 が修正する
        await Execute売上の備考の修正Async(scope, "user003", sales.売上SEQ!.Value);

        var notifications = await scope.App.DbContext.通知DbSet.AsNoTracking().ToListAsync();
        Assert.That(notifications.Select(n => n.宛先), Is.EqualTo(new[] { sales.担当者_従業員番号 }));
        Assert.That(notifications[0].リンク先, Is.EqualTo(販売管理の画面のURL.売上詳細(sales.売上SEQ.ToString()!)));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 担当者本人が売上を修正しても通知しない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(担当者本人が売上を修正しても通知しない));
        await GenerateダミーデータWithout通知Async(scope);
        var sales = await scope.App.DbContext.売上DbSet.AsNoTracking().FirstAsync();

        await Execute売上の備考の修正Async(scope, sales.担当者_従業員番号!, sales.売上SEQ!.Value);

        Assert.That(await scope.App.DbContext.通知DbSet.AnyAsync(), Is.False);
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 在庫を調整したら調整した本人以外の入荷担当の全員に通知する() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(在庫を調整したら調整した本人以外の入荷担当の全員に通知する));
        await GenerateダミーデータWithout通知Async(scope);
        var db = scope.App.DbContext;
        var product = await db.商品DbSet.AsNoTracking().FirstAsync();

        // 入荷担当の user002 が調整する
        await using (var request = await NewRequestAsAsync(scope, "user002")) {
            var messages = new 在庫調整ParameterMessages([], new());
            var context = new OperationContext { ValidationOnly = false };
            await request.Value.Execute在庫調整登録Async(new() {
                商品 = new() { 商品SEQ = product.商品SEQ },
                増減数 = 1,
                在庫調整理由 = "棚卸し",
            }, messages, context);
            Assert.That(messages.HasError(), Is.False, TestFailureMessage.Build("在庫調整に失敗しました。", context, messages));
        }

        var 入荷担当 = await db.従業員DbSet
            .Where(e => e.従業員番号 != "user002" && e.従業員の権限.Any(p => p.権限区分 == 権限区分.入荷担当))
            .Select(e => e.従業員番号)
            .ToListAsync();
        var notifications = await db.通知DbSet.AsNoTracking().ToListAsync();
        Assert.That(入荷担当, Is.Not.Empty);
        Assert.That(notifications.Select(n => n.宛先), Is.EquivalentTo(入荷担当));
        Assert.That(notifications.Select(n => n.リンク先).Distinct(), Is.EqualTo(new[] { 販売管理の画面のURL.商品詳細(product.商品SEQ.ToString()!) }));
    }

    /// <summary>
    /// ダミーデータを作る。ダミーデータの作成でも在庫調整などの業務処理を通るので通知が作られるが、テストで確かめたい通知と混ざらないよう消しておく。
    /// </summary>
    private static async Task GenerateダミーデータWithout通知Async(TestScopeImpl scope) {
        await new DummyDataGeneratorInUnitTest(new OperationContext { ValidationOnly = false }).GenerateAsync(scope.App);
        await scope.App.DbContext.通知DbSet.ExecuteDeleteAsync();
    }

    /// <summary>
    /// 指定したユーザーとして、売上を画面と同じ手順（初期表示の後に修正）で読み込み、備考だけを変えて保存する。
    /// </summary>
    private static async Task Execute売上の備考の修正Async(TestScopeImpl scope, string ログインユーザー, int 売上SEQ) {
        await using var request = await NewRequestAsAsync(scope, ログインユーザー);
        var context = new OperationContext { ValidationOnly = false };

        var loadMessages = new 売上詳細画面初期表示ParameterMessages([], new());
        var detail = await request.Value.Execute売上詳細画面初期表示Async(new() { 売上SEQ = 売上SEQ }, loadMessages, context);
        Assert.That(detail, Is.Not.Null, TestFailureMessage.Build("売上を読み込めませんでした。", context, loadMessages));

        detail!.備考 = "修正";
        var messages = new 売上詳細Messages([], new());
        await request.Value.Execute売上修正Async(detail, messages, context);
        Assert.That(messages.HasError(), Is.False, TestFailureMessage.Build("売上を修正できませんでした。", context, messages));
    }
}

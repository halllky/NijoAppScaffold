using Microsoft.EntityFrameworkCore;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 商品データ取込は画面からはジョブを登録するだけで取込はジョブの中で行う() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(商品データ取込は画面からはジョブを登録するだけで取込はジョブの中で行う));
        var db = scope.App.DbContext;
        await Add従業員Async(db, "ADMIN", 権限区分.システム管理者);

        await using (var request = await NewRequestAsAsync(scope, "ADMIN")) {
            var messages = new MessageSetter([], new());
            var context = new OperationContext { ValidationOnly = false };
            await request.Value.Execute商品データ取込Async(messages, context);
            Assert.That(messages.HasError(), Is.False, TestFailureMessage.Build("商品データ取込のジョブを登録できませんでした。", context, messages));
        }
        Assert.That(await db.商品DbSet.AnyAsync(), Is.False, "取込は、ジョブが実行されるまで行わない");

        var queue = CreateジョブキューFor(scope, OverridedApplicationService.商品データ取込のジョブ);
        await queue.Run次のジョブAsync(CancellationToken.None);

        // モックのサンプルデータには、消費税区分が不正で取り込めない商品が1件ある
        var job = await db.ジョブDbSet.AsNoTracking().SingleAsync();
        Assert.That(job.状態, Is.EqualTo(ジョブの状態.異常終了));
        Assert.That(job.結果メッセージ, Is.EqualTo("2 件のうち、1 件を取り込みました。1 件は取り込めませんでした。理由は取込結果のファイルを参照してください。"));
        Assert.That(await db.商品DbSet.Select(p => p.外部システム側ID).ToListAsync(), Is.EqualTo(new[] { "EX67890" }));

        var csv = await File.ReadAllLinesAsync(Path.Combine(scope.App.Settings.Job.Get作業フォルダ(job.ジョブID!), "spool", "取込結果.csv"));
        Assert.That(csv, Has.Length.EqualTo(3), "見出しと、商品ごとに1行");
        Assert.That(await db.通知DbSet.Select(n => n.宛先).ToListAsync(), Is.EqualTo(new[] { "ADMIN" }), "起動したユーザーに終了を通知する");
    }
}

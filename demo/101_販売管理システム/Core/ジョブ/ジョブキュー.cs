using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyApp.Core.ジョブ;

/// <summary>
/// DB に登録されたジョブのキュー。登録されたジョブを取り出して実行し、定期実行のジョブを予定どおりに登録する。
/// このクラスはポーリングをしない。待機中のジョブや予定の時刻を定期的に確認するのは、呼び出し元の責務である。
/// ジョブの取り出し・本体の実行・結果の記録は、それぞれ別の DI スコープで行う。理由は次の2つ。
/// 本体が失敗して DbContext に保存されない変更が残っても、結果の記録に影響しないようにするため。
/// 現在時刻はスコープごとに固定されるので、開始日時と終了日時をそれぞれの時点の時刻にするため。
/// DI コンテナにはシングルトンで登録する。
/// </summary>
public sealed class ジョブキュー {

    /// <param name="scopeFactory">ジョブの処理ごとの DI スコープを作る</param>
    /// <param name="logger">ジョブの開始と終了のログの出力先</param>
    /// <param name="種類">実行できるジョブの種類。通常は <see cref="OverridedApplicationService"/> に定義されたすべての種類。単体テストではテスト用の種類を渡せる</param>
    /// <exception cref="InvalidOperationException">種類の名前が重複している場合</exception>
    public ジョブキュー(IServiceScopeFactory scopeFactory, ILogger<ジョブキュー> logger, IEnumerable<ジョブの種類> 種類) {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var duplicated = 種類.GroupBy(x => x.Name).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
        if (duplicated.Length > 0) {
            throw new InvalidOperationException($"ジョブの種類の名前が重複しています: {string.Join(", ", duplicated)}");
        }
        _種類 = 種類.ToDictionary(x => x.Name);
    }

    /// <summary>
    /// <see cref="OverridedApplicationService"/> に定義されたすべての種類を扱うキューを作る。
    /// </summary>
    public static ジョブキュー Create(IServiceProvider services) => new(
        services.GetRequiredService<IServiceScopeFactory>(),
        services.GetRequiredService<ILogger<ジョブキュー>>(),
        ジョブの種類.Collect定義された種類());

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ジョブキュー> _logger;
    private readonly IReadOnlyDictionary<string, ジョブの種類> _種類;

    /// <summary>
    /// 待機中のジョブのうち、最も早く登録されたものを1つ取り出して実行し、結果を記録する。
    /// 実行中のログには、処理名としてジョブの種類を、処理1回分の識別子としてジョブ ID を付ける。
    /// </summary>
    /// <param name="cancellationToken">サーバーの停止時にキャンセルされるトークン。ジョブの本体に渡す</param>
    /// <returns>ジョブを実行したら true。待機中のジョブが無ければ false</returns>
    public async Task<bool> Run次のジョブAsync(CancellationToken cancellationToken) {
        取り出したジョブ? taken;
        await using (var scope = _scopeFactory.CreateAsyncScope()) {
            taken = await App(scope).Take次のジョブAsync();
        }
        if (taken == null) return false;

        using var logScope = _logger.BeginScope(new Dictionary<string, string?> {
            [OverridedApplicationService.LOG_SCOPEPROP_LOGIN_USERID] = taken.起動したユーザー,
            [OverridedApplicationService.LOG_SCOPEPROP_PROCESS_NAME] = taken.種類,
            [OverridedApplicationService.LOG_SCOPEPROP_SCOPE_ID] = taken.ジョブID,
        });
        _logger.LogInformation("ジョブを開始します。");

        (ジョブの状態 状態, string? 結果メッセージ) result;
        await using (var scope = _scopeFactory.CreateAsyncScope()) {
            var app = App(scope);
            await app.ActAsジョブAsync(taken.種類, taken.起動したユーザー);
            result = _種類.TryGetValue(taken.種類, out var 種類)
                ? await app.Executeジョブの本体Async(taken, 種類, cancellationToken)
                : app.Fail定義されていない種類のジョブ(taken.種類);
        }
        await Record結果Async(taken.ジョブID, taken.種類, taken.起動したユーザー, result.状態, result.結果メッセージ);

        _logger.LogInformation("ジョブが終了しました。状態: {State}", result.状態);
        return true;
    }

    /// <summary>
    /// 前回のサーバーの停止時に実行中のまま残ったジョブを、中断として異常終了にする。
    /// サーバーの起動時、ジョブの実行を始める前に呼ぶ。中断したジョブの自動での再実行はしない。
    /// </summary>
    public async Task Abort中断されたジョブAsync() {
        string[] ids;
        await using (var scope = _scopeFactory.CreateAsyncScope()) {
            ids = await App(scope).Load中断されたジョブIDAsync();
        }
        foreach (var id in ids) {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var job = await App(scope).DbContext.ジョブDbSet.SingleAsync(j => j.ジョブID == id);
            using var logScope = _logger.BeginScope(new Dictionary<string, string?> {
                [OverridedApplicationService.LOG_SCOPEPROP_LOGIN_USERID] = job.起動したユーザー,
                [OverridedApplicationService.LOG_SCOPEPROP_PROCESS_NAME] = job.種類,
                [OverridedApplicationService.LOG_SCOPEPROP_SCOPE_ID] = job.ジョブID,
            });
            _logger.LogError("前回のサーバーの停止のときに実行中だったジョブを、中断として異常終了にします。");
            await Record結果Async(id, job.種類!, job.起動したユーザー, ジョブの状態.異常終了, MSG.JobInterruptedBeforeRestartResult());
        }
    }

    /// <summary>
    /// 定期実行のジョブの種類のうち、前回の確認日時より後、現在日時以前に予定の時刻があるものを登録する。
    /// 前回の確認日時はプロセスのメモリ上にしか保持しないので、サーバーが停止していた間の予定は登録されない。
    /// </summary>
    /// <param name="前回の確認日時">前回これを呼んだときの戻り値。サーバーの起動後の初回は null を渡す。null のときは何も登録しない</param>
    /// <returns>今回の確認日時。次回の呼び出しに渡す</returns>
    public async Task<DateTime> Register予定のジョブAsync(DateTime? 前回の確認日時) {
        DateTime now;
        await using (var scope = _scopeFactory.CreateAsyncScope()) {
            now = App(scope).CurrentTime;
        }
        if (前回の確認日時 == null) return now;

        foreach (var 種類 in _種類.Values) {
            if (種類.予定?.Has予定の時刻(前回の確認日時.Value, now) != true) continue;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var app = App(scope);
            await app.ActAsジョブAsync(種類.Name, 起動したユーザー: null);
            await using var tran = await app.BeginTransactionAsync();
            var id = await app.RegisterジョブAsync(種類);
            await tran.CommitAsync();
            _logger.LogInformation("定期実行のジョブ「{Kind}」（予定: {Schedule}）を登録しました。ジョブID: {JobId}", 種類.Name, 種類.予定, id);
        }
        return now;
    }

    /// <summary>
    /// ジョブの結果を記録する。更新者はそのジョブとして記録する。
    /// </summary>
    private async Task Record結果Async(string ジョブID, string 種類, string? 起動したユーザー, ジョブの状態 状態, string? 結果メッセージ) {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var app = App(scope);
        await app.ActAsジョブAsync(種類, 起動したユーザー);
        await app.Recordジョブの結果Async(ジョブID, 状態, 結果メッセージ);
    }

    private static OverridedApplicationService App(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<OverridedApplicationService>();
}

/// <summary>
/// キューから取り出して実行中にしたジョブ
/// </summary>
internal sealed record 取り出したジョブ(string ジョブID, string 種類, string? 引数, string? 起動したユーザー);

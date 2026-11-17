using MyApp.Core.ジョブ;

namespace MyApp.WebApi;

/// <summary>
/// WebApi のプロセスの中で常に動き、ジョブを実行する。
/// 起動したら、前回の停止で中断されたジョブを異常終了にしてから、次の2つを並行して繰り返す。
/// <list type="bullet">
/// <item>待機中のジョブを登録の順に取り出して実行する。同時に実行する数は設定で決める</item>
/// <item>定期的なジョブの予定の時刻を過ぎていたら、そのジョブを登録する</item>
/// </list>
/// サーバーの停止のときは、実行中のジョブに停止の合図を渡す。終わるのを待つ時間はホストの停止の待ち時間で、設定で決める。
/// </summary>
internal sealed class JobWorker(ジョブキュー queue, RuntimeSetting settings, ILogger<JobWorker> logger) : BackgroundService {

    /// <summary>
    /// 待機中のジョブが無いときに、次に確かめるまで待つ時間。
    /// 定期的なジョブの予定を確かめる間隔でもあるので、予定の時刻からこの時間だけ遅れて登録されることがある。
    /// </summary>
    private static readonly TimeSpan POLLING_INTERVAL = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        // DB に接続できないなどで失敗しても、ジョブの実行は始める。中断されたジョブが残るだけで、ほかのジョブには影響しないため
        try {
            await queue.Abort中断されたジョブAsync();
        } catch (Exception ex) {
            logger.LogError(ex, "前回の停止で中断されたジョブを、異常終了にできませんでした。");
        }

        await Task.WhenAll([
            RegisterScheduledJobsAsync(stoppingToken),
            .. Enumerable.Range(0, settings.Job.MaxConcurrency).Select(_ => RunJobsAsync(stoppingToken)),
        ]);
    }

    /// <summary>
    /// 待機中のジョブを1件ずつ実行し続ける。
    /// ジョブの本体の例外は結果として記録されるので、ここで失敗するのは DB に接続できないときなど。そのときも止まらずに続ける。
    /// </summary>
    private async Task RunJobsAsync(CancellationToken stoppingToken) {
        var failure = new ContinuousFailure(logger, "ジョブを実行できませんでした。");
        while (!stoppingToken.IsCancellationRequested) {
            try {
                var executed = await queue.Run次のジョブAsync(stoppingToken);
                failure.Recover();
                if (executed) continue;
            } catch (Exception ex) {
                failure.Fail(ex);
            }
            if (!await DelayAsync(stoppingToken)) return;
        }
    }

    /// <summary>
    /// 定期的なジョブの予定を確かめ続ける。前回確かめた日時はメモリーにだけ持つ。
    /// </summary>
    private async Task RegisterScheduledJobsAsync(CancellationToken stoppingToken) {
        var failure = new ContinuousFailure(logger, "定期的なジョブを登録できませんでした。");
        DateTime? checkedAt = null;
        while (!stoppingToken.IsCancellationRequested) {
            try {
                checkedAt = await queue.Register予定のジョブAsync(checkedAt);
                failure.Recover();
            } catch (Exception ex) {
                failure.Fail(ex);
            }
            if (!await DelayAsync(stoppingToken)) return;
        }
    }

    /// <returns>停止の合図があったら false</returns>
    private static async Task<bool> DelayAsync(CancellationToken stoppingToken) {
        try {
            await Task.Delay(POLLING_INTERVAL, stoppingToken);
            return true;
        } catch (OperationCanceledException) {
            return false;
        }
    }

    /// <summary>
    /// 繰り返しの処理の、続けての失敗。
    /// DB の障害の間などに、同じエラーのログが繰り返しの間隔ごとに出続けないよう、続けて失敗している間は最初の1回だけ Error にする。
    /// </summary>
    private sealed class ContinuousFailure(ILogger logger, string message) {
        private bool _failing;

        public void Fail(Exception ex) {
            if (_failing) {
                logger.LogDebug(ex, "{Message}（続けて失敗しています）", message);
            } else {
                logger.LogError(ex, "{Message}", message);
                _failing = true;
            }
        }

        public void Recover() {
            if (!_failing) return;
            logger.LogInformation("回復しました。それまでのエラー: {Message}", message);
            _failing = false;
        }
    }
}

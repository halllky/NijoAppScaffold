using MyApp.Core.ジョブ;

namespace MyApp.WebApi;

/// <summary>
/// WebApi のプロセス内で常駐し、ジョブを実行するバックグラウンドサービス。
/// 起動時に、前回の停止で中断されたジョブを異常終了にしてから、次の2つを並行して繰り返す。
/// <list type="bullet">
/// <item>待機中のジョブを登録順に取り出して実行する。同時実行数は設定ファイルで指定する</item>
/// <item>定期実行のジョブの予定時刻を過ぎていたら、そのジョブを登録する</item>
/// </list>
/// サーバーの停止時は、実行中のジョブをキャンセルする。終了を待つ時間はホストのシャットダウンのタイムアウトで、設定ファイルで指定する。
/// </summary>
internal sealed class JobWorker(ジョブキュー queue, RuntimeSetting settings, ILogger<JobWorker> logger) : BackgroundService {

    /// <summary>
    /// ポーリングの間隔。待機中のジョブが無いときは、この時間だけ待ってから次を確認する。
    /// 定期実行のジョブの予定を確認する間隔でもあるので、予定時刻から最大でこの時間だけ遅れて登録される。
    /// </summary>
    private static readonly TimeSpan POLLING_INTERVAL = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        // DB に接続できないなどの理由で失敗しても、ジョブの実行は開始する。中断されたジョブが実行中のまま残るだけで、ほかのジョブには影響しないため
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
    /// ジョブの本体の例外は結果として記録されるので、ここで例外が発生するのは DB に接続できない場合などに限られる。その場合も止まらずに続ける。
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
    /// 定期実行のジョブの予定を確認し続ける。前回の確認日時はメモリ上にだけ保持する。
    /// </summary>
    private async Task RegisterScheduledJobsAsync(CancellationToken stoppingToken) {
        var failure = new ContinuousFailure(logger, "定期実行のジョブを登録できませんでした。");
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

    /// <returns>サーバーの停止でキャンセルされたら false</returns>
    private static async Task<bool> DelayAsync(CancellationToken stoppingToken) {
        try {
            await Task.Delay(POLLING_INTERVAL, stoppingToken);
            return true;
        } catch (OperationCanceledException) {
            return false;
        }
    }

    /// <summary>
    /// 繰り返し実行する処理の、連続した失敗を扱う。
    /// DB の障害中などに同じエラーのログがポーリングのたびに出力され続けないよう、連続して失敗している間は最初の1回だけ Error にする。
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

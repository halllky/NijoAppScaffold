using System.Text;

namespace MyApp.Core.運用担当への通知;

/// <summary>
/// Error 以上のログを、運用担当者にメールで知らせる。
/// 受け取ったログは貯めておき、自分のスレッドで1秒ごとにメールにして送る。ログを出力した処理は待たされない。
/// 既定では、ログを1件ずつすぐに送る（バッチ向け）。WebApi では <see cref="EnableThrottling"/> で、送るメールの数を抑える。
/// メールの送信は業務処理のトランザクションに参加しない。送信に失敗しても業務処理には影響せず、送り直さない。
/// 失敗は Warning としてログに出す。Error として出すと、それがまた通知の対象になるため。
/// </summary>
public sealed class ErrorNotifier : IDisposable {

    /// <summary>まとめたメールに載せるログの件数</summary>
    public const int MAX_ENTRIES_PER_MAIL = 5;

    /// <summary>
    /// 設定に従って作る。設定を省略していたり、実機を選んでいたりしたら、例外を投げて起動を止める。
    /// </summary>
    public static ErrorNotifier Create(RuntimeSetting settings, string basePath) {
        var notification = settings.ErrorNotification;
        var sender = notification.UseMock switch {
            true => new ErrorMailSenderMock(ResolveMockOutputDirectory(settings, basePath)),

            // 実機の送信の方法（ライブラリ、メールサーバー）は案件で決める。
            // 実機を選んだまま、気付かずにメールが届かない状態で運用するのを防ぐため、起動を止める
            false => throw new InvalidOperationException(
                $"設定 {RuntimeSetting.MY_APP_SECTION}:{nameof(RuntimeSetting.ErrorNotification)}:{nameof(ErrorNotificationSettings.UseMock)} に false が指定されていますが、" +
                $"エラーの通知のメールの実機の送信は実装されていません。"),

            // 省略したときに既定でモックを使うと、本番環境でメールが届かないまま動いていても気付けないため、起動を止める
            null => throw new InvalidOperationException(
                $"設定 {RuntimeSetting.MY_APP_SECTION}:{nameof(RuntimeSetting.ErrorNotification)}:{nameof(ErrorNotificationSettings.UseMock)} がありません。" +
                $"モックを使うなら true を指定してください。"),
        };
        return new ErrorNotifier(notification, sender);
    }

    public ErrorNotifier(ErrorNotificationSettings settings, IErrorMailSender sender) {
        _settings = settings;
        _sender = sender;
        _queue = new ErrorNotificationQueue(MAX_ENTRIES_PER_MAIL);
        _loop = Task.Run(LoopAsync);
    }

    private readonly ErrorNotificationSettings _settings;
    private readonly IErrorMailSender _sender;
    private readonly ErrorNotificationQueue _queue;
    private readonly Lock _lock = new();
    /// <summary>すぐに送ると決まったが、まだ送っていないメール</summary>
    private readonly List<ErrorNotificationBatch> _ready = [];
    private readonly CancellationTokenSource _stopping = new();
    private readonly Task _loop;
    private bool _disposed;

    /// <summary>
    /// 宛先の区分ごとに、送るメールの数を抑えるようにする。間隔は設定で決める。
    /// </summary>
    public void EnableThrottling() {
        lock (_lock) {
            _queue.ThrottleInterval = TimeSpan.FromMinutes(_settings.ThrottleMinutes);
        }
    }

    /// <summary>
    /// 通知の対象のログを受け取る。
    /// </summary>
    public void Enqueue(ErrorLogEntry entry) {
        lock (_lock) {
            if (_disposed) return;
            _ready.AddRange(_queue.Add(entry, DateTime.UtcNow));
        }
    }

    /// <summary>
    /// 送るメールの数を抑えている間に貯めたものも含め、まだ送っていないメールをすべて送る。
    /// </summary>
    public void Flush() {
        List<ErrorNotificationBatch> batches;
        lock (_lock) {
            batches = [.. _ready, .. _queue.TakeAll()];
            _ready.Clear();
        }
        SendAsync(batches, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 送るスレッドを止め、まだ送っていないメールをすべて送る。
    /// </summary>
    public void Dispose() {
        lock (_lock) {
            if (_disposed) return;
            _disposed = true;
        }
        _stopping.Cancel();
        try { _loop.GetAwaiter().GetResult(); } catch (OperationCanceledException) { }
        Flush();
        _stopping.Dispose();
    }

    private async Task LoopAsync() {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await timer.WaitForNextTickAsync(_stopping.Token)) {
            List<ErrorNotificationBatch> batches;
            lock (_lock) {
                batches = [.. _ready, .. _queue.Tick(DateTime.UtcNow)];
                _ready.Clear();
            }
            await SendAsync(batches, _stopping.Token);
        }
    }

    private async Task SendAsync(IEnumerable<ErrorNotificationBatch> batches, CancellationToken cancellationToken) {
        foreach (var batch in batches) {
            try {
                await _sender.SendAsync(ToMail(batch), cancellationToken);
            } catch (Exception ex) when (ex is not OperationCanceledException) {
                NLog.LogManager.GetLogger(typeof(ErrorNotifier).FullName).Warn(ex, "エラーの通知のメールを送れませんでした。");
            }
        }
    }

    private ErrorMail ToMail(ErrorNotificationBatch batch) {
        var to = _settings.DefaultRecipients
            .Concat(_settings.RecipientsByCategory.GetValueOrDefault(batch.Category) ?? [])
            .Distinct()
            .ToArray();

        string subject;
        var body = new StringBuilder();
        if (batch.IsSummary) {
            subject = $"[エラーの通知] {batch.TotalCount}件のエラーが発生しました";
            body.AppendLine($"前回の通知の後に、{batch.TotalCount}件のエラーが発生しました。");
            if (batch.TotalCount > batch.Entries.Count) {
                body.AppendLine($"最初の{batch.Entries.Count}件を載せます。");
            }
        } else {
            var firstLine = batch.Entries[0].Message.Split('\n')[0].Trim();
            subject = $"[エラーの通知] {(firstLine.Length > 60 ? firstLine[..60] + "…" : firstLine)}";
            body.AppendLine("エラーが発生しました。");
        }
        foreach (var entry in batch.Entries) {
            body.AppendLine();
            body.AppendLine("----------------------------------------");
            body.AppendLine($"日時: {entry.Time}");
            body.AppendLine($"レベル: {entry.Level}");
            body.AppendLine($"ログインユーザー ID: {entry.LoginUserId}");
            body.AppendLine($"処理名: {entry.ProcessName}");
            body.AppendLine($"処理1回分の識別子: {entry.ScopeId}");
            body.AppendLine($"ロガー: {entry.LoggerName}");
            body.AppendLine($"本文: {entry.Message}");
            if (!string.IsNullOrEmpty(entry.Exception)) {
                body.AppendLine("例外:");
                body.AppendLine(entry.Exception);
            }
        }
        return new ErrorMail(to, subject, body.ToString());
    }

    private static string ResolveMockOutputDirectory(RuntimeSetting settings, string basePath) {
        var logDirectory = string.IsNullOrWhiteSpace(settings.LogDirectory) ? "Log" : settings.LogDirectory;
        if (!Path.IsPathRooted(logDirectory)) logDirectory = Path.Combine(basePath, logDirectory);
        return Path.Combine(logDirectory, settings.ErrorNotification.MockOutputDirectory);
    }
}

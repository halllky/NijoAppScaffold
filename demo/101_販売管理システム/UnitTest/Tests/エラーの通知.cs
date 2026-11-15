using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MyApp.Core.エラーの通知;
using NLog.Extensions.Logging;

namespace MyApp.UnitTest;

public class エラーの通知 {

    [Test]
    [Category("DB接続なし")]
    public void Error以上のログを運用担当者にメールで知らせる() {
        var (notifier, sender) = CreateNotifier(new ErrorNotificationSettings { DefaultRecipients = ["ops@example.com"] });
        using (var loggerFactory = CreateLoggerFactory(notifier)) {
            var logger = loggerFactory.CreateLogger("テスト");
            logger.LogWarning("警告のログ");
            logger.LogError(new InvalidOperationException("例外の内容"), "エラーのログ");
            logger.LogCritical("致命的なエラーのログ");
        }
        notifier.Dispose();

        var mails = sender.Mails.ToArray();
        Assert.That(mails, Has.Length.EqualTo(2), "Warning は知らせない");
        Assert.That(mails[0].To, Is.EqualTo(new[] { "ops@example.com" }));
        Assert.That(mails[0].Body, Does.Contain("本文: エラーのログ").And.Contain("レベル: ERROR").And.Contain("例外の内容"));
        Assert.That(mails[1].Body, Does.Contain("本文: 致命的なエラーのログ").And.Contain("レベル: FATAL"));
    }

    [Test]
    [Category("DB接続なし")]
    public void 宛先の区分を付けたログは既定の宛先とその区分の宛先に知らせる() {
        var (notifier, sender) = CreateNotifier(new ErrorNotificationSettings {
            DefaultRecipients = ["ops@example.com"],
            RecipientsByCategory = new() { ["商品管理システム"] = ["product@example.com"] },
        });
        using (var loggerFactory = CreateLoggerFactory(notifier)) {
            var logger = loggerFactory.CreateLogger("テスト");
            using (logger.BeginErrorNotificationCategory("商品管理システム")) {
                logger.LogError("区分を付けたログ");
            }
            logger.LogError("区分を付けないログ");
        }
        notifier.Dispose();

        var mails = sender.Mails.ToArray();
        Assert.That(mails.Single(m => m.Body.Contains("区分を付けたログ")).To, Is.EqualTo(new[] { "ops@example.com", "product@example.com" }));
        Assert.That(mails.Single(m => m.Body.Contains("区分を付けないログ")).To, Is.EqualTo(new[] { "ops@example.com" }));
    }

    [Test]
    [Category("DB接続なし")]
    public void 送る数を抑えるときは最初のログをすぐに送り以降は一定時間ごとにまとめて送る() {
        // 一定時間を10分とする。10時から10時30分までエラーが続き、11時に再発した場合
        var queue = new ErrorNotificationQueue(maxEntriesPerMail: 2) { ThrottleInterval = TimeSpan.FromMinutes(10) };
        var t = new DateTime(2026, 1, 1, 10, 0, 0);
        var sent = new List<(DateTime At, ErrorNotificationBatch Batch)>();
        void Add(DateTime at) => sent.AddRange(queue.Add(Entry(), at).Select(b => (at, b)));
        void Tick(DateTime at) => sent.AddRange(queue.Tick(at).Select(b => (at, b)));

        for (var minute = 0; minute <= 30; minute++) {
            Tick(t.AddMinutes(minute));
            Add(t.AddMinutes(minute));
        }
        for (var minute = 31; minute <= 60; minute++) Tick(t.AddMinutes(minute));
        Add(t.AddMinutes(60));

        Assert.That(sent.Select(s => s.At.ToString("HH:mm")), Is.EqualTo(new[] { "10:00", "10:10", "10:20", "10:30", "10:40", "11:00" }));
        Assert.That(sent[0].Batch.IsSummary, Is.False);
        Assert.That(sent[1].Batch.IsSummary, Is.True);
        Assert.That(sent[1].Batch.TotalCount, Is.EqualTo(9), "10:01〜10:09 のログ。10:10 のログは、10:10 に送った後に出力されたので次のまとめに入る");
        Assert.That(sent[1].Batch.Entries, Has.Count.EqualTo(2), "載せるのは最初の数件だけ");
        Assert.That(sent[4].Batch.TotalCount, Is.EqualTo(1), "10:30 に送った後に出力された 10:30 のログ");
        Assert.That(sent[5].Batch.IsSummary, Is.False, "抑えている期間が終わった後のログはすぐに送る");
    }

    [Test]
    [Category("DB接続なし")]
    public void 送る数を抑えないときはログを1件ずつすぐに送る() {
        var queue = new ErrorNotificationQueue(maxEntriesPerMail: 5);
        var t = new DateTime(2026, 1, 1, 10, 0, 0);

        var batches = Enumerable.Range(0, 3).SelectMany(i => queue.Add(Entry(), t)).ToArray();

        Assert.That(batches, Has.Length.EqualTo(3));
        Assert.That(batches.All(b => !b.IsSummary), Is.True);
        Assert.That(queue.TakeAll(), Is.Empty);
    }

    private static ErrorLogEntry Entry() => new("", "", "ERROR", "", "", "", "", "エラー", "");

    private static (ErrorNotifier, FakeSender) CreateNotifier(ErrorNotificationSettings settings) {
        var sender = new FakeSender();
        return (new ErrorNotifier(settings, sender), sender);
    }

    /// <summary>
    /// エラーの通知の出力先だけをもつロガーを作る。並列に実行されるほかのテストのログと混ざらないよう、NLog 全体の設定は使わない。
    /// </summary>
    private static ILoggerFactory CreateLoggerFactory(ErrorNotifier notifier) {
        var config = new NLog.Config.LoggingConfiguration();
        var target = new ErrorNotificationTarget(notifier);
        config.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, target);
        var nlogFactory = new NLog.LogFactory { Configuration = config };
        return LoggerFactory.Create(builder => builder.AddNLog(_ => nlogFactory));
    }

    private sealed class FakeSender : IErrorMailSender {
        public ConcurrentQueue<ErrorMail> Mails { get; } = new();
        public Task SendAsync(ErrorMail mail, CancellationToken cancellationToken) {
            Mails.Enqueue(mail);
            return Task.CompletedTask;
        }
    }
}

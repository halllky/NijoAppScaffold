namespace MyApp.Core.Authorization;

/// <summary>
/// ログインの失敗の記録。入力されたユーザー ID ごとに失敗を数え、
/// 一定期間に一定回数を超えたユーザー ID のログインを、しばらくの間受け付けないようにする。
/// 存在しないユーザー ID の失敗も数える。数えないと、受け付けるかどうかでユーザー ID の有無が分かってしまうため。
/// ユーザー ID は、前後の空白を除き、大文字と小文字を区別せずに数える。
/// DB の照合順序によっては同じユーザーとしてログインできる書き方の違いで、失敗を別々に数えてしまわないようにするため。
/// 記録はサーバーのプロセスのメモリーに持つので、再起動で消え、サーバーが複数台のときは台ごとに数える。
/// IP アドレスごとの制限は行わない。
/// ASP.NET Core のレート制限の機能は、成功したリクエストも含めて数えるので使わず、ログインの失敗だけをこのクラスで数える。
/// アプリケーション全体で1個のインスタンスを使う。
/// </summary>
public sealed class LoginFailureHistory {

    /// <summary>失敗を数える期間</summary>
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
    /// <summary>この回数だけ失敗したら、ログインを受け付けない</summary>
    public const int MAX_FAILURES = 5;
    /// <summary>ログインを受け付けない期間</summary>
    public static readonly TimeSpan BlockDuration = TimeSpan.FromMinutes(15);

    private readonly Dictionary<string, Entry> _entries = [];
    private readonly Lock _lock = new();

    /// <summary>
    /// そのユーザー ID のログインを受け付けない期間中かどうかを返す。
    /// </summary>
    public bool IsBlocked(string userId, DateTime now) {
        lock (_lock) {
            return _entries.TryGetValue(Normalize(userId), out var entry) && entry.BlockedUntil > now;
        }
    }

    /// <summary>
    /// ログインの失敗を記録する。
    /// 失敗を数える期間の中で失敗が規定の回数に達したら、そのユーザー ID のログインを受け付けない期間を始める。
    /// </summary>
    /// <returns>この失敗で、ログインを受け付けない期間が始まった場合は true</returns>
    public bool RecordFailure(string userId, DateTime now) {
        lock (_lock) {
            RemoveExpiredEntries(now);

            var key = Normalize(userId);
            if (!_entries.TryGetValue(key, out var entry)) {
                entry = new Entry();
                _entries[key] = entry;
            }
            entry.Failures.Enqueue(now);
            if (entry.Failures.Count < MAX_FAILURES) return false;

            entry.Failures.Clear();
            entry.BlockedUntil = now + BlockDuration;
            return true;
        }
    }

    /// <summary>
    /// ログインに成功したので、そのユーザー ID の失敗の記録を消す。
    /// </summary>
    public void Clear(string userId) {
        lock (_lock) {
            _entries.Remove(Normalize(userId));
        }
    }

    /// <summary>
    /// 失敗を数える単位にそろえる。
    /// </summary>
    private static string Normalize(string userId) => userId.Trim().ToUpperInvariant();

    /// <summary>
    /// 失敗を数える期間を過ぎた失敗と、受け付けない期間を過ぎた記録を消す。
    /// 多数のユーザー ID で失敗を繰り返されても、メモリーが増え続けないようにするため。
    /// </summary>
    private void RemoveExpiredEntries(DateTime now) {
        foreach (var (userId, entry) in _entries.ToArray()) {
            while (entry.Failures.TryPeek(out var failedAt) && failedAt <= now - Window) {
                entry.Failures.Dequeue();
            }
            if (entry.Failures.Count == 0 && entry.BlockedUntil <= now) {
                _entries.Remove(userId);
            }
        }
    }

    private sealed class Entry {
        public Queue<DateTime> Failures { get; } = new();
        public DateTime BlockedUntil { get; set; } = DateTime.MinValue;
    }
}

namespace MyApp.Core.エラーの通知;

/// <summary>
/// 通知の対象のログを受け取り、いつ、どのログをメールにするかを決める。
/// 送るメールの数を抑える間隔が設定されていれば、宛先の区分ごとに次のように抑える。
/// <list type="bullet">
/// <item>直前の間隔の中で1通も送っていなければ、受け取ったログをすぐに1通にする。</item>
/// <item>そうでなければ貯めておき、前回送ってから間隔が過ぎたときに、貯めたログを1通にまとめる。</item>
/// </list>
/// 間隔が設定されていなければ、受け取ったログを1件ずつすぐにメールにする。
/// 時刻は引数で受け取り、スレッドやタイマーは持たない。スレッドセーフではない。
/// </summary>
public sealed class ErrorNotificationQueue(int maxEntriesPerMail) {

    /// <summary>送るメールの数を抑える間隔。null なら抑えない</summary>
    public TimeSpan? ThrottleInterval { get; set; }

    private readonly Dictionary<string, CategoryState> _states = [];

    /// <summary>
    /// ログを受け取る。すぐに送るべきメールがあれば返す。
    /// </summary>
    public IReadOnlyList<ErrorNotificationBatch> Add(ErrorLogEntry entry, DateTime now) {
        if (ThrottleInterval == null) return [new(entry.Category, [entry], 1, IsSummary: false)];

        if (!_states.TryGetValue(entry.Category, out var state)) {
            state = new CategoryState();
            _states[entry.Category] = state;
        }
        if (state.TotalCount == 0 && (state.LastSentAt == null || now >= state.LastSentAt + ThrottleInterval)) {
            state.LastSentAt = now;
            return [new(entry.Category, [entry], 1, IsSummary: false)];
        }
        if (state.Entries.Count < maxEntriesPerMail) state.Entries.Add(entry);
        state.TotalCount++;
        return [];
    }

    /// <summary>
    /// 前回送ってから間隔が過ぎた宛先の区分について、貯めたログをまとめたメールを返す。定期的に呼ぶ。
    /// </summary>
    public IReadOnlyList<ErrorNotificationBatch> Tick(DateTime now) {
        var result = new List<ErrorNotificationBatch>();
        foreach (var (category, state) in _states) {
            if (state.TotalCount == 0) continue;
            if (state.LastSentAt != null && now < state.LastSentAt + ThrottleInterval) continue;
            result.Add(TakeSummary(category, state));
            state.LastSentAt = now;
        }
        return result;
    }

    /// <summary>
    /// 間隔によらず、貯めたログをすべてまとめたメールを返す。終了するときに使う。
    /// </summary>
    public IReadOnlyList<ErrorNotificationBatch> TakeAll() {
        return [.. _states
            .Where(kv => kv.Value.TotalCount > 0)
            .Select(kv => TakeSummary(kv.Key, kv.Value))];
    }

    private static ErrorNotificationBatch TakeSummary(string category, CategoryState state) {
        var batch = new ErrorNotificationBatch(category, [.. state.Entries], state.TotalCount, IsSummary: true);
        state.Entries.Clear();
        state.TotalCount = 0;
        return batch;
    }

    private sealed class CategoryState {
        public DateTime? LastSentAt { get; set; }
        /// <summary>貯めたログのうち、メールに載せる最初の数件</summary>
        public List<ErrorLogEntry> Entries { get; } = [];
        /// <summary>貯めたログの件数の合計</summary>
        public int TotalCount { get; set; }
    }
}

/// <summary>
/// 1通のメールにするログ。
/// </summary>
/// <param name="Entries">メールに載せるログ。まとめたメールでは最初の数件だけ</param>
/// <param name="TotalCount">このメールで知らせるログの件数の合計</param>
/// <param name="IsSummary">一定時間に出力されたログをまとめたメールかどうか</param>
public sealed record ErrorNotificationBatch(string Category, IReadOnlyList<ErrorLogEntry> Entries, int TotalCount, bool IsSummary);

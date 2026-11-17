namespace MyApp.Core.ジョブ;

/// <summary>
/// 定期的に実行するジョブの予定。「毎日決まった時刻」と「一定の間隔」の2種類だけを扱う。
/// 時刻は業務のタイムゾーンの現地時刻（<see cref="ICurrentTime"/>）である。
/// 曜日や月末などを指定する複雑な予定は扱わない。
/// </summary>
public sealed class ジョブの予定 {

    /// <summary>
    /// 毎日、決まった時刻に実行する。
    /// </summary>
    public static ジョブの予定 毎日(TimeOnly 時刻) => new([時刻], $"毎日 {時刻:HH\\:mm}");

    /// <summary>
    /// 一定の間隔で実行する。毎日 0 時を起点に、間隔ごとの時刻に実行する。
    /// 例えば 7 時間なら 0 時・7 時・14 時・21 時に実行し、翌日はまた 0 時から数える。
    /// </summary>
    /// <param name="間隔">1分以上、1日以下</param>
    public static ジョブの予定 間隔(TimeSpan 間隔) {
        if (間隔 < TimeSpan.FromMinutes(1) || 間隔 > TimeSpan.FromDays(1)) {
            throw new ArgumentOutOfRangeException(nameof(間隔), 間隔, "間隔は1分以上、1日以下で指定してください。");
        }
        var times = new List<TimeOnly>();
        for (var t = TimeSpan.Zero; t < TimeSpan.FromDays(1); t += 間隔) {
            times.Add(TimeOnly.FromTimeSpan(t));
        }
        return new(times, $"{間隔} ごと");
    }

    private ジョブの予定(IReadOnlyList<TimeOnly> 時刻, string 説明) {
        _時刻 = 時刻;
        _説明 = 説明;
    }
    /// <summary>1日の中で実行する時刻</summary>
    private readonly IReadOnlyList<TimeOnly> _時刻;
    private readonly string _説明;

    /// <summary>
    /// 前回の確認日時より後、現在日時以前に、実行予定の時刻があるかどうか。
    /// 前回の確認日時ちょうどの予定は前回の確認で実行済みとみなすので含めず、現在日時ちょうどの予定は含める。
    /// </summary>
    public bool Has予定の時刻(DateTime 前回確かめた日時, DateTime 今) {
        for (var date = DateOnly.FromDateTime(前回確かめた日時); date <= DateOnly.FromDateTime(今); date = date.AddDays(1)) {
            foreach (var time in _時刻) {
                var scheduled = date.ToDateTime(time);
                if (前回確かめた日時 < scheduled && scheduled <= 今) return true;
            }
        }
        return false;
    }

    public override string ToString() => _説明;
}

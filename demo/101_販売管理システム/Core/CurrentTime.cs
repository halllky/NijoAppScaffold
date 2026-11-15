namespace MyApp;

/// <summary>
/// 現在時刻。1回のリクエストの中では、最初に読んだ時点の時刻を返し続ける。
/// 業務のタイムゾーンの現地時刻（<see cref="Value"/>）と UTC（<see cref="UtcValue"/>）は、同じ時点を表す。
/// DIコンテナのスコープ（Webアプリケーションであれば1回のリクエスト）につき1個のインスタンスが作られる。
/// 実際のアプリケーションの要件に応じて適宜修正してよい。
/// </summary>
public class CurrentTime(RuntimeSetting settings) : ICurrentTime {
    /// <summary>
    /// 業務のタイムゾーン（<see cref="RuntimeSetting.BusinessTimeZone"/>）の現地時刻。サーバーの OS のタイムゾーンには依存しない。
    /// タイムゾーンの情報はもたない（<see cref="DateTimeKind.Unspecified"/>）。
    /// このアプリケーションが扱う日時はすべて業務のタイムゾーンの現地時刻なので、保存する日時や、保存済みの日時との比較には、これを使う。
    /// </summary>
    public DateTime Value => _value ??= TimeZoneInfo.ConvertTimeFromUtc(UtcValue, settings.GetBusinessTimeZone());
    private DateTime? _value;

    /// <summary>
    /// UTC の現在時刻。
    /// 保存した日時と比べない、プロセスの中だけで経過時間を数える処理（メモリーで数える回数の制限など）に使う。
    /// 夏時間のあるタイムゾーンでは現地時刻が前後するため。
    /// 保存する日時や、保存済みの日時との比較には使わない。DB の中で現地時刻と UTC が混ざるため。
    /// </summary>
    public DateTime UtcValue => _utcValue ??= DateTime.UtcNow;
    private DateTime? _utcValue;
}

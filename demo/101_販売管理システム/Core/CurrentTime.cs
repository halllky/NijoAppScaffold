namespace MyApp;

/// <summary>
/// 現在時刻。業務のタイムゾーン（<see cref="RuntimeSetting.BusinessTimeZone"/>）の現地時刻を返す。
/// サーバーの OS のタイムゾーンには依存しない。タイムゾーンの情報はもたない（<see cref="DateTimeKind.Unspecified"/>）。
/// このアプリケーションが扱う日時はすべて業務のタイムゾーンの現地時刻なので、保存する日時や、保存済みの日時との比較にも、
/// プロセスの中だけで経過時間を数える処理（メモリーで数える回数の制限など）にも、これを使う。
/// 夏時間のあるタイムゾーンは想定していない。夏時間の切り替えの前後では、現地時刻が前後する。
/// DIコンテナのスコープ（Webアプリケーションであれば1回のリクエスト）につき1個のインスタンスが作られる。
/// 実際のアプリケーションの要件に応じて適宜修正してよい。
/// </summary>
public class CurrentTime(RuntimeSetting settings) : ICurrentTime {
    /// <inheritdoc />
    public DateTime Value => _value ??= TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, settings.GetBusinessTimeZone());
    private DateTime? _value;
}

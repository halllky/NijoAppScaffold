namespace MyApp;

/// <summary>
/// 現在時刻。業務のタイムゾーン（<see cref="RuntimeSetting.BusinessTimeZone"/>）の現地時刻を返す。
/// サーバーの OS のタイムゾーンには依存しない。返す値はタイムゾーンの情報をもたない（<see cref="DateTimeKind.Unspecified"/>）。
/// このアプリケーションが扱う日時はすべて業務のタイムゾーンの現地時刻なので、保存する日時や、保存済みの日時との比較には、これを使う。
/// 保存した日時と比べない、プロセスの中だけで経過時間を数える処理（メモリーで数える回数の制限など）には、UTC の現在時刻を使う。
/// 夏時間のあるタイムゾーンでは現地時刻が前後するため。
/// DIコンテナのスコープ（Webアプリケーションであれば1回のリクエスト）につき1個のインスタンスが作られる。
/// 実際のアプリケーションの要件に応じて適宜修正してよい。
/// </summary>
public class CurrentTime(RuntimeSetting settings) : ICurrentTime {
    /// <inheritdoc />
    public DateTime Value => _value ??= TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, settings.GetBusinessTimeZone());
    private DateTime? _value;
}

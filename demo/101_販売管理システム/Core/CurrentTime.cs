namespace MyApp;

/// <summary>
/// 現在時刻。業務のタイムゾーン（<see cref="RuntimeSetting.BusinessTimeZone"/>）の現地時刻を返す。
/// サーバーの OS のタイムゾーンには依存しない。
/// DIコンテナのスコープ（Webアプリケーションであれば1回のリクエスト）につき1個のインスタンスが作られる。
/// 実際のアプリケーションの要件に応じて適宜修正してよい。
/// </summary>
public class CurrentTime(RuntimeSetting settings) : ICurrentTime {
    /// <inheritdoc />
    public DateTime Value => _value ??= TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, settings.GetBusinessTimeZone());
    private DateTime? _value;
}

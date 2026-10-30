namespace MyApp;

/// <summary>
/// データの更新を記録するときに使う情報。
/// DIコンテナのスコープ（Webアプリケーションであれば1回のリクエスト）につき1個のインスタンスが作られる。
/// 実際のアプリケーションの要件に応じて適宜修正してよい。
/// </summary>
public class AuditInfo : IAuditInfo {
    /// <inheritdoc />
    public DateTime CurrentTime => _currentTime ??= DateTime.Now;
    private DateTime? _currentTime;

    /// <inheritdoc />
    public string CurrentUser => "UNDEFINED";
}

namespace MyApp;

/// <summary>
/// 現在時刻。
/// DIコンテナのスコープ（Webアプリケーションであれば1回のリクエスト）につき1個のインスタンスが作られる。
/// 実際のアプリケーションの要件に応じて適宜修正してよい。
/// </summary>
public class CurrentTime : ICurrentTime {
    /// <inheritdoc />
    public DateTime Value => _value ??= DateTime.Now;
    private DateTime? _value;
}

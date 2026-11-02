namespace MyApp;

/// <summary>
/// 現在操作中のユーザー。
/// DIコンテナのスコープ（Webアプリケーションであれば1回のリクエスト）につき1個のインスタンスが作られる。
/// 実際のアプリケーションの要件に応じて適宜修正してよい。
/// </summary>
public class CurrentUser : ICurrentUser {
    /// <inheritdoc />
    public string Name => "UNDEFINED";
}

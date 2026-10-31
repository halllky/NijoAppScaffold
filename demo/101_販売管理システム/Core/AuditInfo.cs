using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

/// <summary>
/// データの更新を記録するときに使う情報。
/// DIコンテナのスコープ（Webアプリケーションであれば1回のリクエスト）につき1個のインスタンスが作られる。
/// 実際のアプリケーションの要件に応じて適宜修正してよい。
/// </summary>
public class AuditInfo : IAuditInfo {
    /// <summary>ログインユーザーがいない処理（ログイン処理そのものなど）で記録する名前</summary>
    public const string NO_LOGIN_USER = "SYSTEM";

    // DbContext がこのクラスに依存しているので、コンストラクタでは ApplicationService を受け取れない（循環依存になる）
    public AuditInfo(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc />
    public DateTime CurrentTime => _currentTime ??= DateTime.Now;
    private DateTime? _currentTime;

    /// <inheritdoc />
    public string CurrentUser => _serviceProvider.GetRequiredService<OverridedApplicationService>().LoginUser?.従業員番号 ?? NO_LOGIN_USER;
}

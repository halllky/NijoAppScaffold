using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.Core.外部システム.商品管理システム;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 商品管理システムインターフェース。
    /// </summary>
    public I商品管理システム 商品管理システム => _cached商品管理システム ??= ServiceProvider.GetRequiredService<I商品管理システム>();
    private I商品管理システム? _cached商品管理システム;

    /// <summary>
    /// 実行時設定に従い、モック/本番の商品管理システムのクラスを切り替えて登録する。
    /// </summary>
    static partial void Configure外部システム(IServiceCollection services, RuntimeSetting settings, bool isDevelopment, ILogger logger) {
        switch (settings.商品管理システム.UseMock) {
            case true:
                services.AddTransient<I商品管理システム, 商品管理システムMock>();
                if (!isDevelopment) logger.LogWarning("商品管理システムはモックで動いています。");
                break;
            case false:
                services.AddTransient<I商品管理システム, 商品管理システム本番>();
                break;
            case null:
                // 省略したときに既定でモックを使うと、本番環境でモックのまま動いていても気付けないため、起動を止める
                throw new InvalidOperationException(
                    $"設定 {RuntimeSetting.MY_APP_SECTION}:{nameof(RuntimeSetting.商品管理システム)}:{nameof(商品管理システムSettings.UseMock)} がありません。" +
                    "モックを使うなら true、本番の商品管理システムを使うなら false を指定してください。");
        }
    }
}

partial class RuntimeSetting {
    /// <summary>
    /// 商品管理システム連携設定
    /// </summary>
    public 商品管理システムSettings 商品管理システム { get; set; } = new();
}

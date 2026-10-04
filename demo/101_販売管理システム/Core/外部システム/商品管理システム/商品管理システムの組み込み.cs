using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.外部システム.商品管理システム;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 商品管理システムインターフェース。
    /// </summary>
    public I商品管理システム 商品管理システム => _cached商品管理システム ??= ServiceProvider.GetRequiredService<I商品管理システム>();
    private I商品管理システム? _cached商品管理システム;

    /// <summary>
    /// 実行時設定に従い、モック/実際の商品管理システムのクラスを切り替えて登録する。
    /// </summary>
    static partial void Configure外部システム(IServiceCollection services, RuntimeSetting settings) {
        if (settings.商品管理システム.UseMock) {
            services.AddTransient<I商品管理システム, 商品管理システムMock>();
        } else {
            services.AddTransient<I商品管理システム, 商品管理システム本番>();
        }
    }
}

partial class RuntimeSetting {
    /// <summary>
    /// 商品管理システム連携設定
    /// </summary>
    public 商品管理システムSettings 商品管理システム { get; set; } = new();
}

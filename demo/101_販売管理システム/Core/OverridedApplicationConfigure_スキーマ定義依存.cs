using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.外部システム.商品管理システム;

namespace MyApp;

partial class OverridedApplicationConfigure {

    partial void ConfigureDemoService(IServiceCollection services, IConfigurationSection myAppSection) {

        // 商品管理システムの設定をバインド。
        // appsettings.json の設定に従い、モック/実際の外部システムクラスを切り替える。
        if (myAppSection.GetValue<bool>(nameof(商品管理システムSettings.UseMock))) {
            services.AddTransient<I商品管理システム, 商品管理システムMock>();
        } else {
            services.AddTransient<I商品管理システム, 商品管理システム本番>();
        }
    }

    protected override void ConfigureSequenceMember(
        Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder,
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder entity,
        Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<int?> property,
        string sequenceName) {

        // SQLite の場合（SQLiteにはシーケンスがないため、AUTO_INCREMENTを使用）
        property.HasAnnotation("Sqlite:Autoincrement", true);
    }
}

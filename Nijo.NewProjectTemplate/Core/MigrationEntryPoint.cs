using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

/// <summary>
/// Entity Framework Core のデザインタイム用 DbContext ファクトリ。
/// dotnet-ef コマンドがマイグレーション処理を行う際に使用される。
/// </summary>
public partial class MigrationEntryPoint : IDesignTimeDbContextFactory<OverridedDbContext> {
    public OverridedDbContext CreateDbContext(string[] args) {

        // DI コンテナの構築。appsettings.json は WebApi のものを流用する
        var services = new ServiceCollection();
        var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "WebApi"));

        // マイグレーションは開発者が開発環境の設定で行うので、開発環境として扱う
        OverridedApplicationService.ConfigureServices(services, basePath, isDevelopment: true);
        ConfigureServicesスキーマ定義依存(services);

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<OverridedDbContext>();
    }

    /// <summary>
    /// 呼び出し元（Web、バッチ、単体テスト、マイグレーション）ごとに実装すべきサービスのうち、
    /// アプリ固有のものをマイグレーション用に登録する。
    /// 登録すべきものが無ければ実装しなくてよい（実装が無ければ呼び出しごと取り除かれる）。
    /// </summary>
    static partial void ConfigureServicesスキーマ定義依存(IServiceCollection services);
}

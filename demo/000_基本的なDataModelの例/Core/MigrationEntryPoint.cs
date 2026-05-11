using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

/// <summary>
/// Entity Framework Core のデザインタイム用 DbContext ファクトリ。
/// dotnet-ef コマンドがマイグレーション処理を行う際に使用される。
/// </summary>
public class MigrationEntryPoint : IDesignTimeDbContextFactory<OverridedDbContext> {
    public OverridedDbContext CreateDbContext(string[] args) {

        // DI コンテナの構築。appsettings.json は WebApi のものを流用する
        var services = new ServiceCollection();
        var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "WebApi"));

        OverridedApplicationService.ConfigureServices(services, basePath);

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<OverridedDbContext>();
    }
}

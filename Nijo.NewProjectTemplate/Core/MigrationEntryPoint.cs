using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

/// <summary>
/// Entity Framework Core のデザインタイム用 DbContext ファクトリ。
/// dotnet-ef コマンドがマイグレーション処理を行う際に使用される。
/// </summary>
public class MigrationEntryPoint : IDesignTimeDbContextFactory<MyDbContext> {
    public MyDbContext CreateDbContext(string[] args) {

        // DI コンテナの構築。appsettings.json は WebApi のものを流用する
        var services = new ServiceCollection();
        var config = new OverridedApplicationConfigure();
        var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "WebApi"));

        config.ConfigureServices(services, basePath);

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<MyDbContext>();
    }
}

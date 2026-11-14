using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.Authorization;

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

        // 呼び出し元ごとに実装するサービス。DbContext が作成者・更新者の記録のために依存している
        services.AddSingleton<ISessionKeyProvider, SessionKeyProviderInMigration>();
        services.AddSingleton<ICurrentProcess, CurrentProcessInMigration>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<OverridedDbContext>();
    }

    /// <summary>
    /// マイグレーションにおける <see cref="ICurrentProcess"/>。
    /// </summary>
    private class CurrentProcessInMigration : ICurrentProcess {
        public string Name => "Migration";
    }

    /// <summary>
    /// マイグレーションにおける <see cref="ISessionKeyProvider"/>。
    /// マイグレーションはログインユーザーのいない処理なので、セッションキーは常に無い。
    /// </summary>
    private class SessionKeyProviderInMigration : ISessionKeyProvider {
        public string? GetSessionKey() => null;
        public void ClearSessionKey() { }
        public void ReturnSessionKeyToClient(string sessionKey) {
            throw new InvalidOperationException("マイグレーション中にログインすることはできません。");
        }
    }
}

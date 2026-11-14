using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.Authorization;

namespace MyApp;

partial class MigrationEntryPoint {

    static partial void ConfigureServicesスキーマ定義依存(IServiceCollection services) {
        // DbContext が作成者・更新者の記録のためにこれらに依存している
        services.AddSingleton<ISessionKeyProvider, SessionKeyProviderInMigration>();
        services.AddSingleton<ICurrentProcess, CurrentProcessInMigration>();
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

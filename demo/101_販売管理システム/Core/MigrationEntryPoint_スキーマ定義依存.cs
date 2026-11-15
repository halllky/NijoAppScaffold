using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.Authorization;

namespace MyApp;

partial class MigrationEntryPoint {

    static partial void ConfigureServicesスキーマ定義依存(IServiceCollection services) {
        // DbContext が作成者・更新者の記録のためにこれに依存している
        services.AddSingleton<ICurrentUser, CurrentUserInMigration>();
    }

    private class CurrentUserInMigration : ICurrentUser {
        public string Name => "Migration";
    }
}

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MyApp.WebApi;

/// <summary>
/// 死活の確認のうち、DB に接続できるかの確認。
/// 外部システムやディスクの空きなど、DB 以外は確認しない。
/// </summary>
internal sealed class DbHealthCheck(OverridedDbContext dbContext) : IHealthCheck {
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) {
        return await dbContext.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("DB に接続できません。");
    }
}

namespace ConnectorCore.Infrastructure.HealthCheck;

using ConnectorCore.Database;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

/// <summary>
/// Health check for Sql Database.
/// </summary>
internal class HealthCheckSql : HealthCheckBase<DbConnectionProvider>
{
    /// <summary>
    /// Failing calls.
    /// </summary>
    public static readonly HealthCheckResult ConnectionFailed = HealthCheckResult.Degraded("Failed to connect");
}

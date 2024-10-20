namespace Willow.IoTService.Deployment.Dashboard.Application.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;
using Willow.IoTService.Deployment.DataAccess.Db;

/// <summary>
///     A health check for Sql Database.
/// </summary>
public class HealthCheckSql : HealthCheckBase<DeploymentDbContext>
{
    /// <summary>
    ///     Failing calls.
    /// </summary>
    public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");
}

using Authorization.TwinPlatform.Persistence.Contexts;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

namespace Authorization.TwinPlatform.HealthChecks;
public class HealthCheckSqlServer : HealthCheckBase<TwinPlatformAuthContext>
{
    /// <summary>
    /// Status indicates Sql Server is not configured
    /// </summary>
    public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Not configured");

    /// <summary>
    /// Status indicates calls to Sql Server fails
    /// </summary>
    public static HealthCheckResult FailingCalls => HealthCheckResult.Degraded("Failing calls");
}

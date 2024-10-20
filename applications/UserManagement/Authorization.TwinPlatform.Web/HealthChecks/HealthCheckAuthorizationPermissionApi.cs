using Authorization.TwinPlatform.Common.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

namespace Authorization.TwinPlatform.Web.HealthChecks;


public class HealthCheckAuthorizationPermissionApi : HealthCheckBase<UserAuthorizationService>
{
    /// <summary>
    /// Status indicates url not configured for Authorization Permission Api
    /// </summary>
    public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Not configured");

    /// <summary>
    /// Status indicates calls to Authorization Permission Api fails
    /// </summary>
    public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");
}

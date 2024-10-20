using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

namespace RulesEngine.Web.Services;

/// <summary>
/// A health check for the authorization serice
/// </summary>
public class HealthCheckAuthorizationService : HealthCheckBase<IPolicyDecisionService>
{
    /// <summary>
    /// Not configured
    /// </summary>
    public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Authorization Service not configured");

    /// <summary>
    /// Failing requests
    /// </summary>
    public static readonly HealthCheckResult FailingRequests = HealthCheckResult.Degraded("Authorization Service failing requests");
}

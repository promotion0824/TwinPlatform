using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.HealthChecks;

namespace Willow.AzureDigitalTwins.Api.Diagnostic;

/// <summary>
/// A health check for ADX
/// </summary>
public class HealthCheckADX : HealthCheckBase<HealthService>
{
    /// <summary>
    /// Failing calls
    /// </summary>
    public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");
}

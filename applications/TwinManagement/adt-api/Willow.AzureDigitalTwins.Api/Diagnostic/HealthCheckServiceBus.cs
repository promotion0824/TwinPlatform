using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.HealthChecks;

namespace Willow.AzureDigitalTwins.Api.Diagnostic;

/// <summary>
/// A health check for Service Bus
/// </summary>
public class HealthCheckServiceBus : HealthCheckBase<HealthService>
{
    /// <summary>
    /// Failed to connect
    /// </summary>
    public static readonly HealthCheckResult FailedToConnect = HealthCheckResult.Degraded("Failed to connect");
}

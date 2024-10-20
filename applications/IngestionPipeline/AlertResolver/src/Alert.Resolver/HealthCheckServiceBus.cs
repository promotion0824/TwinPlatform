using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

namespace Willow.Alert.Resolver;

/// <summary>
/// A health check for Service Bus
/// </summary>
public class HealthCheckServiceBus : HealthCheckBase<string>
{
    /// <summary>
    /// Failed to connect
    /// </summary>
    public static readonly HealthCheckResult FailedToConnect = HealthCheckResult.Degraded("Failed to connect");
}

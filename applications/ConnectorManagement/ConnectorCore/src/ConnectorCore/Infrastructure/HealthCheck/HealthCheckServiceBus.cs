namespace ConnectorCore.Infrastructure.HealthCheck;

using ConnectorCore.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

/// <summary>
/// A health check for Service Bus.
/// </summary>
internal class HealthCheckServiceBus : HealthCheckBase<IConnectorsService>
{
    /// <summary>
    /// Failing calls.
    /// </summary>
    public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");
}

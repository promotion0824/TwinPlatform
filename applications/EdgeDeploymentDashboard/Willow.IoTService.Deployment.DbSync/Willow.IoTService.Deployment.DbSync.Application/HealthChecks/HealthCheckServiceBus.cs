namespace Willow.IoTService.Deployment.DbSync.Application.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

/// <summary>
///     A health check for Service Bus.
/// </summary>
public class HealthCheckServiceBus : HealthCheckBase<ConnectorSyncConsumer>
{
    /// <summary>
    ///     Failing calls.
    /// </summary>
    public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");
}

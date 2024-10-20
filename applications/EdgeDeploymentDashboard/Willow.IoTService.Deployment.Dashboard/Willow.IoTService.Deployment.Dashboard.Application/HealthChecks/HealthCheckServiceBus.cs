namespace Willow.IoTService.Deployment.Dashboard.Application.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;
using Willow.IoTService.Deployment.Dashboard.Application.PortServices;

/// <summary>
///     A health check for Service Bus.
/// </summary>
public class HealthCheckServiceBus : HealthCheckBase<IDeployModuleService>
{
    /// <summary>
    ///     Failing calls.
    /// </summary>
    public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");
}

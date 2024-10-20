namespace Willow.IoTService.Deployment.Service.Application.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;
using Willow.IoTService.Deployment.Service.Application.Deployments;

/// <summary>
///     A health check for Service Bus.
/// </summary>
public class HealthCheckServiceBus : HealthCheckBase<DeployModuleConsumer>
{
    /// <summary>
    ///     Failing calls.
    /// </summary>
    public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");
}

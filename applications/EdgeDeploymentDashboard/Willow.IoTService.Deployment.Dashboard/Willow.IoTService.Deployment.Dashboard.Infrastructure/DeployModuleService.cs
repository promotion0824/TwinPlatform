namespace Willow.IoTService.Deployment.Dashboard.Infrastructure;

using MassTransit;
using Microsoft.Extensions.Logging;
using Willow.IoTService.Deployment.Common;
using Willow.IoTService.Deployment.Common.Messages;
using Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateDeployment;
using Willow.IoTService.Deployment.Dashboard.Application.HealthChecks;
using Willow.IoTService.Deployment.Dashboard.Application.PortServices;

/// <inheritdoc />
public class DeployModuleService(ILogger<DeployModuleService> logger, IBus bus, HealthCheckServiceBus healthCheckServiceBus)
    : IDeployModuleService
{
    /// <inheritdoc />
    public async Task SendStatusAsync(
        Guid deploymentId,
        Guid moduleId,
        DeploymentStatus status,
        string? message = null,
        DateTimeOffset? appliedDateTime = null,
        CancellationToken cancellationToken = default)
    {
        var result = new
        {
            DeploymentId = deploymentId,
            ModuleId = moduleId,
            AppliedDateTime = appliedDateTime ?? DateTimeOffset.UtcNow,
            Status = status,
            Message = message,
        };
        try
        {
            await bus.Publish<IDeploymentStatus>(
                                                 result,
                                                 context => { context.TimeToLive = TimeSpan.FromMinutes(1); },
                                                 cancellationToken);
        }
        catch (Exception)
        {
            healthCheckServiceBus.Current = HealthCheckServiceBus.FailingCalls;
            throw;
        }

        healthCheckServiceBus.Current = HealthCheckServiceBus.Healthy;
        logger.LogDebug("Deployment status updated: {@Status}", result);
    }

    /// <inheritdoc />
    public async Task SendDeployModuleMessageAsync(
        Guid deploymentId,
        Guid moduleId,
        string version,
        IDictionary<string, ContainerConfiguration>? containerConfigs = null,
        bool? isBaseDeployment = null,
        CancellationToken cancellationToken = default)
    {
        var address = bus.Address;
        var endpointAddressBuilder = new UriBuilder(address) { Path = "deploy-module" };
        try
        {
            var endpoint = await bus.GetSendEndpoint(endpointAddressBuilder.Uri);
            await endpoint.Send<IDeployModule>(
                                               new
                                               {
                                                   DeploymentId = deploymentId,
                                                   ModuleId = moduleId,
                                                   Version = version,
                                                   ContainerConfigs = containerConfigs,
                                                   IsBaseDeployment = isBaseDeployment ?? false,
                                               },
                                               cancellationToken);
        }
        catch (Exception)
        {
            healthCheckServiceBus.Current = HealthCheckServiceBus.FailingCalls;
            throw;
        }

        healthCheckServiceBus.Current = HealthCheckServiceBus.Healthy;
    }
}

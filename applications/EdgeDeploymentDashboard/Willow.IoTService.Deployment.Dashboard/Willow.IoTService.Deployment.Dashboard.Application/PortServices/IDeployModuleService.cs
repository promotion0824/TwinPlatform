namespace Willow.IoTService.Deployment.Dashboard.Application.PortServices;

using Willow.IoTService.Deployment.Common;
using Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateDeployment;

/// <summary>
///     Service to send messages to the deploy-module endpoint.
/// </summary>
public interface IDeployModuleService
{
    /// <summary>
    ///     Sends status of the deployment to the service bus.
    /// </summary>
    /// <param name="deploymentId">ID of the deployment.</param>
    /// <param name="moduleId">ID of the module being deployed.</param>
    /// <param name="status">Status of the deployment request.</param>
    /// <param name="message">Additional message associated with the status.</param>
    /// <param name="appliedDateTime">Date the deployment was applied.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>A <see cref="Task" /> for sending status.</returns>
    Task SendStatusAsync(
        Guid deploymentId,
        Guid moduleId,
        DeploymentStatus status,
        string? message = null,
        DateTimeOffset? appliedDateTime = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sends the deployment request to the deploy-module queue.
    /// </summary>
    /// <param name="deploymentId">ID of the deployment.</param>
    /// <param name="moduleId">ID of the module being deployed.</param>
    /// <param name="version">Module config version to be deployed.</param>
    /// <param name="containerConfigs">Optional containerConfig options.</param>
    /// <param name="isBaseDeployment">Indicates whether the deployment is for base modules.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>A <see cref="Task" /> representing sending the deployment message to service bus.</returns>
    Task SendDeployModuleMessageAsync(
        Guid deploymentId,
        Guid moduleId,
        string version,
        IDictionary<string, ContainerConfiguration>? containerConfigs = null,
        bool? isBaseDeployment = null,
        CancellationToken cancellationToken = default);
}

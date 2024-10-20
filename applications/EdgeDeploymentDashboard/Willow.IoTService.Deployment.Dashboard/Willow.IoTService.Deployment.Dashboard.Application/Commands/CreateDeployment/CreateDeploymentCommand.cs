namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateDeployment;

using System.ComponentModel;
using JetBrains.Annotations;
using MediatR;
using Willow.IoTService.Deployment.Common;
using Willow.IoTService.Deployment.Common.Messages;
using Willow.IoTService.Deployment.Dashboard.Application.AuditLogging;
using Willow.IoTService.Deployment.DataAccess.Services;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record CreateDeploymentCommand : IRequest<DeploymentDto>, IAuditLog
{
    public Guid ModuleId { get; init; }

    [DefaultValue("1.0.0")]
    public string Version { get; init; } = "1.0.0";

    /// <summary>
    ///     Gets a key value pair of parameters that will be used to generate the deployment manifest.
    ///     The key should be the module name in the deployment template.
    /// </summary>
    public IDictionary<string, ContainerConfiguration>? ContainerConfigs { get; init; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record ContainerConfiguration : IContainerConfiguration
{
    public string? Image { get; init; }

    public ModuleRunStates? RunState { get; init; }

    /// <summary>
    ///     Gets the environment variables of the format "KEY=VALUE".
    /// </summary>
    public IEnumerable<string>? EnvironmentVariables { get; init; }
}

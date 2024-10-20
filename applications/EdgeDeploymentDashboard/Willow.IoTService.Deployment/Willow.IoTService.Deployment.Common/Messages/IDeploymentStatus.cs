using MassTransit;

namespace Willow.IoTService.Deployment.Common.Messages;

[EntityName("deployment-status")]
public interface IDeploymentStatus
{
    Guid DeploymentId { get; }

    Guid ModuleId { get; }

    DateTimeOffset DateTimeApplied { get; }

    DeploymentStatus Status { get; }

    string? Message { get; }
}

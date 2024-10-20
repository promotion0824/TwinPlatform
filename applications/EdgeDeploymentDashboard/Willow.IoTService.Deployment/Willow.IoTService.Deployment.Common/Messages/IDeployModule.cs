namespace Willow.IoTService.Deployment.Common.Messages;

public interface IDeployModule
{
    Guid DeploymentId { get; }

    Guid ModuleId { get; }

    string Version { get; }

    IReadOnlyDictionary<string, IContainerConfiguration>? ContainerConfigs { get; }

    bool IsBaseDeployment { get; }
}

public interface IContainerConfiguration
{
    string? Image { get; }

    ModuleRunStates? RunState { get; }

    IEnumerable<string>? EnvironmentVariables { get; }
}

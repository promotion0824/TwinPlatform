namespace Willow.IoTService.Deployment.DataAccess.Services;

public record DeploymentCreateInput(
    Guid ModuleId,
    string Status,
    string StatusMessage,
    string Version,
    string AssignedBy,
    DateTimeOffset DateTimeApplied);

namespace Willow.IoTService.Deployment.DataAccess.Services;

public record DeploymentStatusUpdateInput(
    Guid Id,
    string Status,
    string StatusMessage,
    DateTimeOffset DateTimeApplied);

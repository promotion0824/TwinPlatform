using Willow.IoTService.Deployment.DataAccess.Entities;

namespace Willow.IoTService.Deployment.DataAccess.Services;

public record ModuleDto(
    Guid Id,
    string Name,
    string ModuleType,
    bool IsArchived,
    bool IsSynced,
    bool? IsAutoDeployment,
    string? DeviceName,
    string? IoTHubName,
    string? Environment,
    Platforms? Platform,
    string? Version,
    string? AssignedBy,
    string? Status,
    string? StatusMessage,
    DateTimeOffset? DateTimeApplied)
{
    public static ModuleDto CreateFrom(ModuleEntity moduleEntity,
                                       DeploymentEntity? deploymentEntity = null)
    {
        return new ModuleDto(moduleEntity.Id,
                             moduleEntity.Name,
                             moduleEntity.ModuleType,
                             moduleEntity.IsArchived,
                             moduleEntity.IsSynced,
                             moduleEntity.Config?.IsAutoDeployment,
                             moduleEntity.Config?.DeviceName,
                             moduleEntity.Config?.IoTHubName,
                             moduleEntity.Config?.Environment,
                             moduleEntity.Config?.Platform,
                             deploymentEntity?.Version,
                             deploymentEntity?.AssignedBy,
                             deploymentEntity?.Status,
                             deploymentEntity?.StatusMessage,
                             deploymentEntity?.DateTimeApplied);
    }
}

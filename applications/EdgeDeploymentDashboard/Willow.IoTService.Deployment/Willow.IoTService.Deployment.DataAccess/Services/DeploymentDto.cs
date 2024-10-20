using Willow.IoTService.Deployment.DataAccess.Entities;

namespace Willow.IoTService.Deployment.DataAccess.Services;

public record DeploymentDto(
    Guid Id,
    string Name,
    string Version,
    string AssignedBy,
    string Status,
    string StatusMessage,
    DateTimeOffset DateTimeCreated,
    DateTimeOffset DateTimeApplied,
    Guid ModuleId,
    string ModuleName,
    string ModuleType,
    string? DeviceName)

{
    public static DeploymentDto CreateFrom(DeploymentEntity entity)
    {
        return new DeploymentDto(entity.Id,
                                 entity.Name,
                                 entity.Version,
                                 entity.AssignedBy,
                                 entity.Status,
                                 entity.StatusMessage,
                                 entity.CreatedOn,
                                 entity.DateTimeApplied,
                                 entity.Module.Id,
                                 entity.Module.Name,
                                 entity.Module.ModuleType,
                                 entity.Module.Config?.DeviceName);
    }
}

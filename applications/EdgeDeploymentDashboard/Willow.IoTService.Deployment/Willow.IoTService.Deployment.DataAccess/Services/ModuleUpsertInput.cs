namespace Willow.IoTService.Deployment.DataAccess.Services;

public record ModuleUpsertInput(
    string Name,
    string ModuleType,
    bool IsArchived,
    bool IsSynced = true,
    Guid? Id = null);

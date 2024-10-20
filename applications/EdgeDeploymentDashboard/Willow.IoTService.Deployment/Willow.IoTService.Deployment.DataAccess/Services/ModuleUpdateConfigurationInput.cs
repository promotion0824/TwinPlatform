using Willow.IoTService.Deployment.DataAccess.Entities;

namespace Willow.IoTService.Deployment.DataAccess.Services;

public record ModuleUpdateConfigurationInput(
    Guid Id,
    bool? IsAutoDeployment = null,
    string? DeviceName = null,
    string? IoTHubName = null,
    string? Environment = null,
    Platforms? Platform = null);

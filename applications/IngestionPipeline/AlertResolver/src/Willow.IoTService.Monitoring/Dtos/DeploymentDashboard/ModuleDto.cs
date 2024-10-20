using System;
using System.Collections.Generic;

namespace Willow.IoTService.Monitoring.Dtos.DeploymentDashboard;

public record ModuleDto(Guid Id,
                        Guid SiteId,
                        string Name,
                        string ModuleType,
                        string? DeviceName,
                        string? IoTHubName,
                        string? Environment);


public record PagedResult
{
    public int TotalCount { get; init; }
    public IEnumerable<ModuleDto>? Items { get; init; }
}
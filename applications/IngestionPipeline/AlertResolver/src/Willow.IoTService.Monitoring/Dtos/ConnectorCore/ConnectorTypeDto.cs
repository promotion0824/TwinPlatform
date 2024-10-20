using System;

namespace Willow.IoTService.Monitoring.Dtos.ConnectorCore;

public record ConnectorTypeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
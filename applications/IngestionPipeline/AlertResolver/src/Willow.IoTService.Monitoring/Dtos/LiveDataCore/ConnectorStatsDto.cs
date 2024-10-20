using System;
using System.Collections.Generic;

namespace Willow.IoTService.Monitoring.Dtos.LiveDataCore;

public class ConnectorStatsResult
{
    public IEnumerable<ConnectorStatsDto>? Data { get; init; }
}
public class ConnectorStatsDto
{
    public Guid ConnectorId { get; init; }
    public string? CurrentSetState { get; init; }
    public string? CurrentStatus { get; init; }
    public int TotalTelemetryCount { get; init; }
}
namespace Willow.LiveData.Core.Features.Telemetry.DTOs;

using System.Collections.Generic;

/// <summary>
/// Telemetry Multi-state response data.
/// </summary>
public class TelemetryMultiStateResponseData : TelemetrySummaryData
{
    /// <summary>
    /// Gets the state.
    /// </summary>
    public Dictionary<string, int> State { get; init; }
}

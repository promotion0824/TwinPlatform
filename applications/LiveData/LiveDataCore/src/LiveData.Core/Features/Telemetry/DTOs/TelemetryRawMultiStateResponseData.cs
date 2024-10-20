namespace Willow.LiveData.Core.Features.Telemetry.DTOs;

/// <summary>
/// Telemetry raw multi-state response data.
/// </summary>
public class TelemetryRawMultiStateResponseData : TelemetrySummaryData
{
    /// <summary>
    /// Gets the state.
    /// </summary>
    public Newtonsoft.Json.Linq.JObject State { get; init; }
}

namespace Willow.LiveData.Core.Features.Telemetry.DTOs;

/// <summary>
/// Telemetry binary response data.
/// </summary>
public class TelemetryBinaryResponseData : TelemetrySummaryData
{
    /// <summary>
    /// Gets or sets the on count.
    /// </summary>
    public int OnCount { get; set; }

    /// <summary>
    /// Gets or sets the off count.
    /// </summary>
    public int OffCount { get; set; }
}

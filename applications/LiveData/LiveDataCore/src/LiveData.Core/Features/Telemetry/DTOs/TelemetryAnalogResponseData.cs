namespace Willow.LiveData.Core.Features.Telemetry.DTOs;

/// <summary>
/// Telemetry Analog Response Data.
/// </summary>
public class TelemetryAnalogResponseData : TelemetrySummaryData
{
    /// <summary>
    /// Gets or sets the average.
    /// </summary>
    public double? Average { get; set; }

    /// <summary>
    /// Gets or sets the minimum.
    /// </summary>
    public double? Minimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum.
    /// </summary>
    public double? Maximum { get; set; }
}

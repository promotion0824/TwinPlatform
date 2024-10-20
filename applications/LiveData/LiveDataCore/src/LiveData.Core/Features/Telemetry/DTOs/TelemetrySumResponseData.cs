namespace Willow.LiveData.Core.Features.Telemetry.DTOs;

/// <summary>
/// Telemetry sum response data.
/// </summary>
public class TelemetrySumResponseData : TelemetrySummaryData
{
    /// <summary>
    /// Gets or sets the sum.
    /// </summary>
    public double? Sum { get; set; }
}

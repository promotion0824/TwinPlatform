namespace Willow.LiveData.Core.Features.Telemetry.DTOs;

using System;

/// <summary>
/// Telemetry summary data.
/// </summary>
public class TelemetrySummaryData : TelemetryBaseIdData
{
    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the value is out of range.
    /// </summary>
    public bool? IsValueOutOfRange { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the value is invalid.
    /// </summary>
    public bool? IsInvalid { get; set; }
}

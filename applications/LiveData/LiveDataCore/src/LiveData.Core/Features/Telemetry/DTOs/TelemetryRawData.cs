namespace Willow.LiveData.Core.Features.Telemetry.DTOs;

using System;

/// <summary>
/// Telemetry raw data.
/// </summary>
public class TelemetryRawData : TelemetryBaseIdData
{
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }
}

namespace Willow.LiveData.Core.Features.Telemetry.DTOs;

using System.Collections.Generic;

/// <summary>
/// Represents the raw telemetry result.
/// </summary>
public class TelemetryRawResult
{
    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    public List<TelemetryRawData> Data { get; set; }

    /// <summary>
    /// Gets or sets the continuation token.
    /// </summary>
    public string ContinuationToken { get; set; }
}

namespace Willow.LiveData.Core.Features.Telemetry.DTOs;

/// <summary>
/// The base class for telemetry data with an ID.
/// </summary>
public class TelemetryBaseIdData
{
    /// <summary>
    /// Gets or sets the Twin ID.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the external ID.
    /// </summary>
    public string ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the trend ID.
    /// </summary>
    public string TrendId { get; set; }
}

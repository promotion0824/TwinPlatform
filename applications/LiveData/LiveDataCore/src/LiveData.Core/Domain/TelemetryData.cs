namespace Willow.LiveData.Core.Domain;

using System;

/// <summary>
/// Represents telemetry data.
/// </summary>
public class TelemetryData
{
    /// <summary>
    /// Gets or sets the ConnectorId.
    /// </summary>
    public string ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the TwinId.
    /// </summary>
    public string TwinId { get; set; }

    /// <summary>
    /// Gets or sets the ExternalId.
    /// </summary>
    public string ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the TrendId.
    /// </summary>
    public string TrendId { get; set; }

    /// <summary>
    /// Gets or sets the SourceTimestamp.
    /// </summary>
    public DateTime SourceTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the EnqueuedTimestamp.
    /// </summary>
    public DateTime EnqueuedTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the Value.
    /// </summary>
    public dynamic ScalarValue { get; set; }

    /// <summary>
    /// Gets or sets the Latitude.
    /// </summary>
    public decimal Latitude { get; set; }

    /// <summary>
    /// Gets or sets the Longitude.
    /// </summary>
    public decimal Longitude { get; set; }

    /// <summary>
    /// Gets or sets the Altitude.
    /// </summary>
    public decimal Altitude { get; set; }

    /// <summary>
    /// Gets or sets the Properties bag.
    /// </summary>
    public dynamic Properties { get; set; }
}

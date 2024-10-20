namespace Willow.LiveData.Pipeline;

/// <summary>
/// A representation of the input to the telemetry ingestion pipeline.
/// </summary>
public record Telemetry
{
    /// <summary>
    /// Gets the connector ID.
    /// </summary>
    public string? ConnectorId { get; init; }

    /// <summary>
    /// Gets the trend ID.
    /// </summary>
    [Obsolete("Avoid Trend ID and use DtId")]
    public Guid? TrendId { get; init; }

    /// <summary>
    /// Gets the timestamp of when the telemetry was generated.
    /// </summary>
    public DateTime SourceTimestamp { get; init; }

    /// <summary>
    /// Gets or sets the timestamp of when the telemetry was added to the queue.
    /// </summary>
    public DateTime EnqueuedTimestamp { get; set; }

    /// <summary>
    /// Gets the digital twin ID.
    /// </summary>
    public string? DtId { get; init; }

    /// <summary>
    /// Gets the external ID of the twin.
    /// </summary>
    public string? ExternalId { get; init; }

    /// <summary>
    /// Gets the value of the telemetry.
    /// </summary>
    public dynamic? ScalarValue { get; init; }

    /// <summary>
    /// Gets the latitude of the device.
    /// </summary>
    public double? Latitude { get; init; }

    /// <summary>
    /// Gets the longitude of the device.
    /// </summary>
    public double? Longitude { get; init; }

    /// <summary>
    /// Gets the Altitude of the device.
    /// </summary>
    public double? Altitude { get; init; }

    /// <summary>
    /// Gets or sets a dynamic object containing any additional properties.
    /// </summary>
    public dynamic? Properties { get; set; }
}

namespace Willow.LiveData.Core.Features.TimeSeries.Models;

/// <summary>
/// Incoming telemetry data.
/// </summary>
public record IncomingTelemetry
{
    /// <summary>
    /// Gets the connector ID.
    /// </summary>
    public string ConnectorId { get; init; }

    /// <summary>
    /// Gets the timestamp of when the telemetry was generated.
    /// </summary>
    public required DateTime SourceTimestamp { get; init; }

    /// <summary>
    /// Gets the digital twin ID.
    /// </summary>
    public string DtId { get; init; }

    /// <summary>
    /// Gets the external ID of the twin.
    /// </summary>
    public string ExternalId { get; init; }

    /// <summary>
    /// Gets the value of the telemetry.
    /// </summary>
    public required double ScalarValue { get; init; }

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
    public dynamic Properties { get; set; }
}

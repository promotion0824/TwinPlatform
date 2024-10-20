namespace Willow.LiveData.TelemetryDataQuality.Models;

internal record TwinDetails
{
    /// <summary>
    /// Gets the digital twin ID.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the Model ID of the twin.
    /// </summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// Gets the connector ID.
    /// </summary>
    public string? ConnectorId { get; init; }

    /// <summary>
    /// Gets the external ID of the twin.
    /// </summary>
    public required string ExternalId { get; init; }

    /// <summary>
    /// Gets the Unit property of the twin.
    /// </summary>
    public string? Unit { get; init; }

    /// <summary>
    /// Gets the Trend Interval property of the twin.
    /// </summary>
    public int? TrendInterval { get; init; }
}

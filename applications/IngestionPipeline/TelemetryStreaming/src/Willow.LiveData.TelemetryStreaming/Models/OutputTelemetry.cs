namespace Willow.LiveData.TelemetryStreaming.Models;

using System.Text.Json;

/// <summary>
/// The format for telemetry sent to the streaming pipeline.
/// </summary>
public readonly struct OutputTelemetry
{
    /// <summary>
    /// Gets the source timestamp.
    /// </summary>
    public readonly DateTime SourceTimestamp { get; init; }

    /// <summary>
    /// Gets the enqueued timestamp.
    /// </summary>
    public readonly DateTime EnqueuedTimestamp { get; init; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public readonly double Value { get; init; }

    /// <summary>
    /// Gets the external identifier.
    /// </summary>
    public readonly string ExternalId { get; init; }

    /// <summary>
    /// Gets the connector identifier.
    /// </summary>
    public readonly string ConnectorId { get; init; }

    /// <summary>
    /// Gets any metadata associated with this message.
    /// </summary>
    public readonly object? Metadata { get; init; }

    /// <summary>
    /// Returns the current instance as a JSON string.
    /// </summary>
    /// <returns>A JSON string.</returns>
    public readonly override string ToString() => JsonSerializer.Serialize(this);
}

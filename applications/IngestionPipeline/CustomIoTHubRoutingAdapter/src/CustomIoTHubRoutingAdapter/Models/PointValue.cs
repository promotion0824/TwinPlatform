namespace Willow.CustomIoTHubRoutingAdapter.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Point value format for telemetry message with v1.0 version.
/// </summary>
internal record PointValue
{
    [JsonPropertyName("v")]
    public double? Value { get; init; }

    [JsonPropertyName("t")]
    public DateTime Timestamp { get; init; }

    [JsonPropertyName("pid")]
    public string? PointId { get; init; }

    [JsonPropertyName("pextid")]
    public string? PointExternalId { get; init; }

    [JsonPropertyName("cid")]
    public string? ConnectorId { get; init; }
}

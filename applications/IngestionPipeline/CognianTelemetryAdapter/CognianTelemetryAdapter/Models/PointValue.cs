namespace Willow.CognianTelemetryAdapter.Models;
/// <summary>
/// Point value format for telemetry message with v1.0 version.
/// </summary>
internal record PointValue
{
    public object? Value { get; init; }

    public DateTime Timestamp { get; init; }

    public string? PointExternalId { get; init; }
}

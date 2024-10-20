namespace Willow.MappedTelemetryAdaptor.Models;

internal record MappedInput
{
    public string? PointId { get; init; }

    public DateTime Timestamp { get; init; }

    public dynamic? Value { get; init; }
}

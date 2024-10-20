using System.Text.Json;

namespace Common;
public struct RawData
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public RawData()
    {
    }

    public DateTime SourceTimestamp { get; set; }

    public DateTime EnqueuedTimestamp { get; set; }

    public double Value { get; set; }

    public required string ExternalId { get; set; }

    public required string ConnectorId { get; set; }

    public object? Metadata { get; set; }

    public override readonly string ToString() => JsonSerializer.Serialize(this, _options);
}

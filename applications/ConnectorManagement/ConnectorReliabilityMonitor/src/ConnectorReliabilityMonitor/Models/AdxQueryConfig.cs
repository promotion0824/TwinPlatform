namespace Willow.ConnectorReliabilityMonitor.Models;

using System.Text.Json.Serialization;

internal class AdxQueryConfig
{
    public int ConnectorUpdateIntervalSeconds { get; set; } = 60 * 5;

    [JsonIgnore]
    public TimeSpan RunInterval => TimeSpan.FromSeconds(ConnectorUpdateIntervalSeconds);
}

internal class AdxQueryConfigItem
{
    public required string Key { get; set; }

    public string? Description { get; set; }

    public required Func<Dictionary<string, string>, string> Query { get; set; }

    public MetricType MetricType { get; set; }
}

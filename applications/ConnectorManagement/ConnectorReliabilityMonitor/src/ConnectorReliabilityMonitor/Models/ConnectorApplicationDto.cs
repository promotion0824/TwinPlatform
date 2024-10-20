namespace Willow.ConnectorReliabilityMonitor.Models;

using Newtonsoft.Json.Linq;

internal record ConnectorApplicationDto
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public int Interval { get; set; } = 300;

    public string ConnectorType { get; init; } = string.Empty;

    public string Source => ConnectorType.Contains("willow-edge") ? "iot" : ConnectorType.Contains("Cognian") ? "cognian" : "mapped";

    private JArray? Buildings { get; init; }

    public List<string?> BuildingList => Buildings?.Select(b => b.Value<string>()).ToList() ?? [];
}

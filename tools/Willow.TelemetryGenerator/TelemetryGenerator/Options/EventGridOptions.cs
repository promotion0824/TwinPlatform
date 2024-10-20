namespace Willow.TelemetryGenerator.Options;

internal record EventGridOptions : GlobalOptions
{
    public required Uri TopicEndpoint { get; set; }

    public string? Key { get; set; }

    public string? ErrorType { get; set; }
}

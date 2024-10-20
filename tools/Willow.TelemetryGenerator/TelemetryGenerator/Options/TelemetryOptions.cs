namespace Willow.TelemetryGenerator.Options;

internal record TelemetryOptions : GlobalOptions
{
    public required string ConnectorId { get; set; }
    public required string ExternalId { get; set; }
    public required string DtId { get; set; }
}

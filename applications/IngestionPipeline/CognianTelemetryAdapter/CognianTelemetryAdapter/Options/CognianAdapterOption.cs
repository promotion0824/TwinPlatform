namespace Willow.CognianTelemetryAdapter.Options;

internal sealed class CognianAdapterOption
{
    public const string Section = "CognianAdapter";

    public required string ConnectorId { get; set; }
}

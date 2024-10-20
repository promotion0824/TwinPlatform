namespace Willow.ConnectorReliabilityMonitor.Models;

internal record AdxResultDto
{
    public string? ConnectorId { get; init; }

    public long Count { get; set; }
}

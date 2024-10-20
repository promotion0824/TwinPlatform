namespace Willow.IoTService.Monitoring.Options;

public record ServiceKeyOptions
{
    public const string SectionName = "ServiceKeyAuth";
    public string ServiceKey1 { get; init; } = string.Empty;
}

namespace Willow.IoTService.Monitoring.Options;

public record M2MOptions
{
    public const string SectionName = "M2M";
    public string AuthDomain { get; init; } = string.Empty;
    public string AuthClientId { get; init; } = string.Empty;
    public string AuthClientSecret { get; init; } = string.Empty;
    public string AuthAudience { get; init; } = string.Empty;
}

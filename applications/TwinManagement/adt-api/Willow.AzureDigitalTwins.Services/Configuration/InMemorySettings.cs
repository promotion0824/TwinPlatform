namespace Willow.AzureDigitalTwins.Services.Configuration;

public class InMemorySettings
{
    public InMemorySourceType Source { get; set; }
    public LocalSystemSettings LocalSystem { get; set; }
    public bool Sync { get; set; }
    public int TwinCacheExpirationMinutes { get; set; } = TimeSpan.FromHours(1).Minutes;
    public int ExtendedTwinCacheExpirationMinutes { get; set; } = TimeSpan.FromHours(12).Minutes;
    public int ModelCacheExpirationMinutes { get; set; } = TimeSpan.FromDays(3).Minutes;
    public bool Lazy { get; set; }
}

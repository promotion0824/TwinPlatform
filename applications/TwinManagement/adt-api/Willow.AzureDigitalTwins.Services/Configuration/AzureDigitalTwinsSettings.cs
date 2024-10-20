namespace Willow.AzureDigitalTwins.Services.Configuration;

public class AzureDigitalTwinsSettings
{
    public AzureDigitalTwinSourceType SourceType { get; set; }
    public InMemorySettings InMemory { get; set; }
    public InstanceSettings Instance { get; set; }
    public int? PercentDegreeOfParallelism { get; set; }
}

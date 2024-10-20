using Willow.AzureDigitalTwins.Services.Cache.Models;

namespace Willow.AzureDigitalTwins.Services.Interfaces;

public interface IAzureDigitalTwinCache
{
    public IModelCache ModelCache { get; }
    public ITwinCache TwinCache { get; }
}

using Willow.AzureDigitalTwins.Services.Interfaces;

namespace Willow.AzureDigitalTwins.Services.Cache.Models;

public class AzureDigitalTwinCache : IAzureDigitalTwinCache
{
    public IModelCache ModelCache { get; init; }

    public ITwinCache TwinCache { get; init; }

    public AzureDigitalTwinCache(IModelCache modelCache, ITwinCache twinCache)
    {
        ModelCache = modelCache;
        TwinCache = twinCache;
    }
}

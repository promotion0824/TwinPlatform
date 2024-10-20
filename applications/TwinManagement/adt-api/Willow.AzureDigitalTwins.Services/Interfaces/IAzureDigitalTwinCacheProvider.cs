using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Interfaces;

public interface IAzureDigitalTwinCacheProvider
{
    IAzureDigitalTwinCache GetOrCreateCache(bool useExtendedCache=false);

    bool IsCacheReady(bool triggerLoad = true);
    Task InitializeCache();

    Task<bool> ClearCacheAsync(IEnumerable<EntityType> entityTypes);

    Task<IAzureDigitalTwinCache> RefreshCacheAsync();
}

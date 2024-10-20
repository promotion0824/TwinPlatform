using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using Willow.AzureDigitalTwins.Services.Cache.Models;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Cache.Providers;

public abstract class BaseCacheProvider<T> : IAzureDigitalTwinCacheProvider where T : class
{
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    protected InMemorySettings Settings { get; }

    protected IMemoryCache MemoryCache { get; }

    protected string CacheModelKey { get { return $"Cache_Model_{Settings.Source}"; } }

    protected string CacheTwinKey { get { return $"Cache_Twin_{Settings.Source}"; } }

    protected string ExtendedCacheTwinKey { get { return $"{CacheTwinKey}_Extended"; } }

    protected ILogger Logger { get; }

    public BaseCacheProvider(InMemorySettings settings,
        IMemoryCache memoryCache,
        ILogger<T> logger)
    {
        Settings = settings;
        MemoryCache = memoryCache;
        Logger = logger;
    }

    private async Task<IModelCache> GetOrCreateModelCacheAsync()
    {
        return (await MemoryCache.GetOrCreateAsync<IModelCache>(CacheModelKey,
                       async (entity) =>
                       {
                           entity.SetAbsoluteExpiration(TimeSpan.FromMinutes(Settings.ModelCacheExpirationMinutes));

                           Logger.LogInformation("Loading Model cache from {Source}, {ModelCacheExpirationMinutes} minutes expiration...", Settings.Source, Settings.ModelCacheExpirationMinutes);
                           var models = await GetModelsAsync();
                           Logger.LogInformation("Done loading models, {Count} models retrieved...", models.Count);

                           return new ModelCache(models, Logger);
                       }))!;
    }

    private async Task GetOrCreateTwinCacheAsync()
    {
        // Get Or Create the general twin Cache
        await MemoryCache.GetOrCreateAsync<ITwinCache>(CacheTwinKey,
                       async (entity) =>
                       {
                           entity.SetAbsoluteExpiration(TimeSpan.FromMinutes(Settings.TwinCacheExpirationMinutes));

                           Logger.LogInformation("Loading Twin cache from {Source}, {TwinCacheExpirationMinutes} minutes expiration...", Settings.Source, Settings.TwinCacheExpirationMinutes);

                           ConcurrentDictionary<string, BasicDigitalTwin> twins = new();
                           ConcurrentDictionary<string, HashSet<string>> twinsByModel = new();
                           var loadTwins = Task.Run(async () =>
                           {
                               Logger.LogInformation("Loading twins...");
                               await ProcessTwinsAsync(twins, twinsByModel);
                               Logger.LogInformation("Done loading twins, {Count} twins retrieved...", twins.Count);
                           });

                           ConcurrentDictionary<string, BasicRelationship> relationships = new();
                           ConcurrentDictionary<string, List<string>> twinRelationships = new();
                           ConcurrentDictionary<string, List<string>> twinIncomingRelationships = new();
                           var loadRelationships = Task.Run(async () =>
                           {
                               Logger.LogInformation("Loading relationships...");
                               await ProcessRelationshipsAsync(relationships, twinRelationships, twinIncomingRelationships);
                               Logger.LogInformation("Done loading relationships, {Count} relationships retrieved...", relationships.Count);
                           });

                           await Task.WhenAll(loadTwins, loadRelationships);

                           return new TwinCache(twins, relationships, twinRelationships, twinsByModel, twinIncomingRelationships);
                       });

        // Get or Create the extended twin cache
        MemoryCache.GetOrCreate<ITwinCache>(ExtendedCacheTwinKey, (entry) =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(Settings.ExtendedTwinCacheExpirationMinutes));
            Logger.LogInformation("Extended Twin cache is set to {TwinCacheExpirationMinutes} minutes expiration...", Settings.ExtendedTwinCacheExpirationMinutes);
            return new TwinCache(new(), new(), new(), new(), new());
        });
    }

    public async Task InitializeCache()
    {
        try
        {
            // Adding Thread Safety in the event when a different thread cleared cache and is about to initialize cache
            await _semaphore.WaitAsync();
            var watch = Stopwatch.StartNew();
            var loadModelCache = GetOrCreateModelCacheAsync();
            var loadTwinCache = GetOrCreateTwinCacheAsync();

            await Task.WhenAll(loadModelCache, loadTwinCache);

            watch.Stop();
            Logger.LogInformation("Done loading cache in {Minutes} min...", Math.Round(watch.Elapsed.TotalMinutes, 2));
        }
        finally
        {
            _semaphore.Release();
        }

    }

    protected abstract Task ProcessTwinsAsync(ConcurrentDictionary<string, BasicDigitalTwin> twins, ConcurrentDictionary<string, HashSet<string>> twinsByModel);

    protected abstract Task ProcessRelationshipsAsync(ConcurrentDictionary<string, BasicRelationship> relationships, ConcurrentDictionary<string, List<string>> twinRelationships, ConcurrentDictionary<string, List<string>> twinIncomingRelationships);

    protected abstract Task<ConcurrentDictionary<string, DigitalTwinsModelBasicData>> GetModelsAsync();

    public IAzureDigitalTwinCache GetOrCreateCache(bool useExtendedCache)
    {
        if (!IsCacheReady(false))
            InitializeCache().Wait();

        return new AzureDigitalTwinCache(MemoryCache.Get<IModelCache>(CacheModelKey)!, MemoryCache.Get<ITwinCache>(useExtendedCache ? ExtendedCacheTwinKey : CacheTwinKey)!);
    }

    public bool IsCacheReady(bool triggerLoad = true)
    {
        bool isAllCacheLoaded = MemoryCache.TryGetValue(CacheModelKey, out _) && MemoryCache.TryGetValue(CacheTwinKey, out _)
                                && MemoryCache.TryGetValue(ExtendedCacheTwinKey, out _);
        if (isAllCacheLoaded)
            return isAllCacheLoaded;

        if (!isAllCacheLoaded && triggerLoad)
            // Trigger load cache and not wait for it. IsCacheReady is meant to indicate if cache has been loaded
            InitializeCache().ConfigureAwait(false);

        return isAllCacheLoaded;
    }

    protected void ProcessRelationshipsMapAsync(ConcurrentDictionary<string, BasicRelationship> relationships,
        ConcurrentDictionary<string, List<string>> twinRelationships,
        ConcurrentDictionary<string, List<string>> twinIncomingRelationships,
        IEnumerable<BasicRelationship> sourceRelationships,
        Action<BasicRelationship> customProcess = null)
    {
        Parallel.ForEach(sourceRelationships, x =>
        {
            if (!twinRelationships.ContainsKey(x.SourceId))
                twinRelationships.TryAdd(x.SourceId, new List<string>());

            if (!twinIncomingRelationships.ContainsKey(x.TargetId))
                twinIncomingRelationships.TryAdd(x.TargetId, new List<string>());

            twinRelationships[x.SourceId].Add(x.Id);
            twinIncomingRelationships[x.TargetId].Add(x.Id);

            var relationshipId = x.Id;

            if (customProcess != null)
                customProcess(x);

            relationships.TryAdd(relationshipId, x);
        });
    }

    public async Task<bool> ClearCacheAsync(IEnumerable<EntityType> entityTypes)
    {
        foreach (var entityType in entityTypes)
        {
            switch (entityType)
            {

                case EntityType.Models:
                    //Check the existing cache for Model entry and remove it
                    if (MemoryCache.TryGetValue(CacheModelKey, out _))
                    {
                        MemoryCache.Remove(CacheModelKey);
                        Logger.LogInformation("All Models cleared from the Cache");
                    }
                    break;
                case EntityType.Twins:
                case EntityType.Relationships:
                    //Check the existing cache for Twin entry and remove it
                    if (MemoryCache.TryGetValue(CacheTwinKey, out _))
                    {
                        MemoryCache.Remove(CacheTwinKey);
                        Logger.LogInformation("All Twins and Relationships cleared from the Cache");
                    }
                    if (MemoryCache.TryGetValue(ExtendedCacheTwinKey, out _))
                    {
                        MemoryCache.Remove(ExtendedCacheTwinKey);
                        Logger.LogInformation("All Twins and Relationships cleared from the extended Cache");
                    }
                    break;
            }
        }

        return await Task.FromResult(true);
    }

    public async Task<IAzureDigitalTwinCache> RefreshCacheAsync()
    {
        await ClearCacheAsync(new List<EntityType>() { EntityType.Models, EntityType.Twins, EntityType.Relationships });
        //Reinitialize the Cache
        var cache = GetOrCreateCache(false);

        return await Task.FromResult(cache);
    }
}

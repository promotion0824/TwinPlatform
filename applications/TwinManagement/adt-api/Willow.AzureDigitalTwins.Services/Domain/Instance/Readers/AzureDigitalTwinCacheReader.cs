using Azure.Core;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Willow.AzureDigitalTwins.Api.Services.Domain.Instance.Readers;
using Willow.AzureDigitalTwins.Services.Cache.Models;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Domain.Instance.Readers;

public class AzureDigitalTwinCacheReader : AzureDigitalTwinReader, IAzureDigitalTwinReaderCacheSelector
{
    private readonly IAzureDigitalTwinCacheProvider _azureDigitalTwinCacheProvider;
    private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;
    private readonly ILogger<AzureDigitalTwinCacheReader> _logger;

    private Lazy<IAzureDigitalTwinCache> _azureDigitalTwinCache;
    private IAzureDigitalTwinCache AzureDigitalTwinCache => _azureDigitalTwinCache.Value;

    public AzureDigitalTwinCacheReader(
        InstanceSettings settings,
        TokenCredential tokenCredential,
        ILogger<AzureDigitalTwinCacheReader> logger,
        IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider,
        IAzureDigitalTwinModelParser azureDigitalTwinModelParser) : base(settings, tokenCredential, logger)
    {
        _azureDigitalTwinCacheProvider = azureDigitalTwinCacheProvider;
        _azureDigitalTwinModelParser = azureDigitalTwinModelParser;
        _logger = logger;
        SetCacheType(useExtended: false);
    }

    public override bool IsServiceReady()
    {
        return _azureDigitalTwinCacheProvider.IsCacheReady();
    }

    public async override Task<BasicDigitalTwin> GetDigitalTwinAsync(string twinId)
    {
        if (AzureDigitalTwinCache.TwinCache.Twins.TryGetValue(twinId, out var twinValue))
            return twinValue;

        var twin = await base.GetDigitalTwinAsync(twinId);

        if (twin != null)
            AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceTwin(twin);

        return twin;
    }

    public async override Task<BasicRelationship> GetRelationshipAsync(string relationshipId, string twinId)
    {
        if (AzureDigitalTwinCache.TwinCache.Relationships.TryGetValue(relationshipId, out var relationshipValue))
            return relationshipValue;

        var relationship = await base.GetRelationshipAsync(relationshipId, twinId);

        if (relationship != null)
            AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceRelationship(relationship);

        return relationship;
    }

    /// <summary>
    /// Get the relationships for a given twin, checking the cache first and otherwise going to ADT at most once,
    ///   regardless of the number of tasks trying to make this call for the same twinId.
    /// </summary>
    public override async Task<IEnumerable<BasicRelationship>> GetTwinRelationshipsAsync(string twinId, string relationshipName = null)
    {
        IEnumerable<BasicRelationship> relationships = null;

        // Check to see if another thread has already provided the results we need to satisfy this call
        bool TryGetReturn(out IEnumerable<BasicRelationship> rels)
        {
            rels = null;
            if (AzureDigitalTwinCache.TwinCache.LoadedTwinsRelationships.Contains(twinId))
            {
                rels = GetCacheRelationships(twinId, AzureDigitalTwinCache.TwinCache.TwinRelationships)
                                .Where(x => relationshipName == null || x.Name == relationshipName)
                                // This ToList was added to avoid a collection-modified-during-enumeration race-condition 
                                //   which now appears to be fixed with the locks on typeof(TwinCache) here and in TwinCache.cs.
                                // However, it's cheap insurance in case there is a corner-case we didn't find
                                .ToList();
            }
            return rels != null;
        }

        // Check the cache before we try and create/use a fetch task
        if (TryGetReturn(out relationships))
            return relationships;

        try
        {
            relationships = await OnceOnly<IEnumerable<BasicRelationship>>.Execute(twinId, () =>
            {
                return base.GetTwinRelationshipsAsync(twinId);
            });
        }
        catch (Exception)
        {
            _logger.LogTrace("Error while retrieving relationships for twin Id :{TwinId}", twinId);
            throw;
        }

        lock (typeof(TwinCache))
        {
            // Atomically update Incoming and Outgoing rel caches and mark as loaded for twinId 
            foreach (var relationship in relationships)
                AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceRelationship(relationship);

            AzureDigitalTwinCache.TwinCache.LoadedTwinsRelationships.Add(twinId);
        }

        // After code in above lock, we are guaranteed to have results
        if (TryGetReturn(out relationships))
            return relationships;

        throw new InvalidOperationException("CacheReader in invalid state");
    }

    public async override Task<IEnumerable<BasicRelationship>> GetIncomingRelationshipsAsync(string twinId)
    {
        if (AzureDigitalTwinCache.TwinCache.LoadedTwinsIncomingRelationships.Contains(twinId))
            return GetCacheRelationships(twinId, AzureDigitalTwinCache.TwinCache.TwinIncomingRelationships);

        var relationships = await base.GetIncomingRelationshipsAsync(twinId);
        foreach (var relationship in relationships)
            AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceRelationship(relationship);

        AzureDigitalTwinCache.TwinCache.LoadedTwinsIncomingRelationships.Add(twinId);

        return relationships;
    }

    private IEnumerable<BasicRelationship> GetCacheRelationships(
        string twinId,
        ConcurrentDictionary<string, List<string>> twinRelationships)
    {
        try
        {
            if (twinRelationships.TryGetValue(twinId, out var relationships))
            {
                return relationships.Select(relId => AzureDigitalTwinCache.TwinCache.Relationships[relId]);
            }

            return Enumerable.Empty<BasicRelationship>();
        }
        catch (Exception ex)
        {
            // TODO: This race condition should be fixed, but keep instrumented until verified
            _logger.LogError(ex, "GetCacheRelationship: error getting cached rels");
            throw;
        }
    }

    protected async override Task<IEnumerable<BasicRelationship>> GetSourceRelationshipsAsync(IEnumerable<string> twinIds, bool include)
    {
        if (!include)
            return Enumerable.Empty<BasicRelationship>();

        var loadedRelationships = twinIds.Where(x => AzureDigitalTwinCache.TwinCache.LoadedTwinsRelationships.Contains(x))
            .SelectMany(x => GetCacheRelationships(x, AzureDigitalTwinCache.TwinCache.TwinRelationships));
        var twinRelationshipsToLoad = twinIds.Where(x => !AzureDigitalTwinCache.TwinCache.LoadedTwinsRelationships.Contains(x));

        if (!twinRelationshipsToLoad.Any())
            return loadedRelationships;

        var missingRelationships = await base.GetSourceRelationshipsAsync(twinIds, include);
        foreach (var relationship in missingRelationships)
            AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceRelationship(relationship);

        foreach (var twinId in twinRelationshipsToLoad)
            AzureDigitalTwinCache.TwinCache.LoadedTwinsRelationships.Add(twinId);

        return loadedRelationships.Union(missingRelationships);
    }

    protected async override Task<IEnumerable<BasicRelationship>> GetTargetRelationshipsAsync(IEnumerable<string> twinIds, bool include)
    {
        if (!include)
            return Enumerable.Empty<BasicRelationship>();

        var loadedRelationships = twinIds.Where(x => AzureDigitalTwinCache.TwinCache.LoadedTwinsIncomingRelationships.Contains(x))
            .SelectMany(x => GetCacheRelationships(x, AzureDigitalTwinCache.TwinCache.TwinIncomingRelationships));
        var twinRelationshipsToLoad = twinIds.Where(x => !AzureDigitalTwinCache.TwinCache.LoadedTwinsIncomingRelationships.Contains(x));

        if (!twinRelationshipsToLoad.Any())
            return loadedRelationships;

        var missingRelationships = await base.GetTargetRelationshipsAsync(twinIds, include);
        foreach (var relationship in missingRelationships)
            AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceRelationship(relationship);

        foreach (var twinId in twinRelationshipsToLoad)
            AzureDigitalTwinCache.TwinCache.LoadedTwinsIncomingRelationships.Add(twinId);

        return loadedRelationships.Union(missingRelationships);
    }

    public async override Task<DigitalTwinsModelBasicData> GetModelAsync(string modelId)
    {
        if (AzureDigitalTwinCache.ModelCache.ModelInfos.TryGetValue(modelId, out var modelValue))
            return modelValue.ToModelBasicData();

        return await base.GetModelAsync(modelId);
    }

    public async override Task<IEnumerable<DigitalTwinsModelBasicData>> GetModelsAsync(string rootModelId = null)
    {
        if (AzureDigitalTwinCache.ModelCache.IsModelsLoaded)
        {
            if (!string.IsNullOrEmpty(rootModelId))
            {
                var interfaceDescendants = _azureDigitalTwinModelParser.GetInterfaceDescendants(new List<string> { rootModelId });
                return AzureDigitalTwinCache.ModelCache.ModelInfos
                    .Where(x => interfaceDescendants.ContainsKey(x.Key))
                    .Select(x => x.Value.ToModelBasicData());
            }

            return AzureDigitalTwinCache.ModelCache.ModelInfos.Select(x => x.Value.ToModelBasicData());
        }

        var models = await base.GetModelsAsync(rootModelId);

        AzureDigitalTwinCache.ModelCache.TryCreateOrReplaceModel(models);

        AzureDigitalTwinCache.ModelCache.IsModelsLoaded = string.IsNullOrEmpty(rootModelId);

        return models;
    }

    /// <summary>
    /// Sets the type of cache to use for twin caching.
    /// </summary>
    /// <param name="useExtended">True to use extended twin cache.</param>
    /// <returns>Returns true if use extended</returns>
    public bool SetCacheType(bool useExtended = false)
    {

        // Sets the type of cache to use for any further operation with this instance
        // AzureDigitalTwinCacheReader is a scoped instance, so new instance created per request and set cache type changes will not be retained between different request

        // We currently support two different cache types
        // 1. General Cache
        // 2.Extended Cache (extended cache basically has long timeouts to cache location twins)

        // UseExtended = false -> General Cache will be selected for read/write through twin cache operations
        // UseExtended = true -> Extended cache will be selected for read/write through twin cache operations
        _azureDigitalTwinCache = new Lazy<IAzureDigitalTwinCache>(_azureDigitalTwinCacheProvider.GetOrCreateCache(useExtended));
        return useExtended;
    }
}

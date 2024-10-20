using Azure;
using Azure.DigitalTwins.Core;
using System.Collections.Concurrent;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.AzureDigitalTwins.Services.Interfaces;

namespace Willow.AzureDigitalTwins.Services.Domain.InMemory.Readers;

public class InMemoryInstanceLazyTwinReader : InMemoryTwinReader
{
    private readonly IAzureDigitalTwinReader _azureDigitalTwinReader;

    public InMemoryInstanceLazyTwinReader(IAzureDigitalTwinModelParser azureDigitalTwinModelParser,
        IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider,
        IAzureDigitalTwinReader azureDigitalTwinReader) : base(azureDigitalTwinModelParser, azureDigitalTwinCacheProvider)
    {
        _azureDigitalTwinReader = azureDigitalTwinReader;
    }

    public async override Task<IEnumerable<BasicRelationship>> GetTwinRelationshipsAsync(string twinId, string relationshipName = null)
    {
        if (!AzureDigitalTwinCache.TwinCache.TwinRelationships.TryGetValue(twinId, out var relations))
        {
            return Enumerable.Empty<BasicRelationship>();
        }

        var relationships = await GetOrLoadRelationshipsAsync(relations.ToList());

        return relationships.Where(x => relationshipName == null || x.Name == relationshipName);
    }

    public async override Task<IEnumerable<BasicRelationship>> GetIncomingRelationshipsAsync(string twinId)
    {
        if (!AzureDigitalTwinCache.TwinCache.TwinIncomingRelationships.TryGetValue(twinId, out var relations))
        {
            return Enumerable.Empty<BasicRelationship>();
        }

        return await GetOrLoadRelationshipsAsync(relations.ToList());
    }

    protected async override Task<IEnumerable<BasicDigitalTwin>> GetTwinsFromTargetIdsAsync(IEnumerable<string> targetTwinIds)
    {
        return await GetOrLoadTwinsAsync(targetTwinIds);
    }

    private async Task<IEnumerable<BasicDigitalTwin>> GetOrLoadTwinsAsync(IEnumerable<string> targetTwinIds)
    {
        return await GetOrLoadEntitiesAsync(targetTwinIds, AzureDigitalTwinCache.TwinCache.Twins, x => x != null,
            x => AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceTwin(x),
            async x =>
            {
                var page = await _azureDigitalTwinReader.GetTwinsAsync(twinIds: x);
                return await page.FetchAll(p => _azureDigitalTwinReader.GetTwinsAsync(twinIds: x, continuationToken: p.ContinuationToken));
            });
    }

    public async override Task<IEnumerable<BasicRelationship>> GetRelationshipsAsync(IEnumerable<string> ids = null)
    {
        var targetIds = ids != null && ids.Any() ? AzureDigitalTwinCache.TwinCache.Relationships.Where(x => ids.Contains(x.Key)).Select(x => x.Key)
            : AzureDigitalTwinCache.TwinCache.Relationships.Select(x => x.Key);

        return await GetOrLoadRelationshipsAsync(targetIds);
    }

    private async Task<IEnumerable<BasicRelationship>> GetOrLoadRelationshipsAsync(IEnumerable<string> targetRelationshipIds)
    {
        return await GetOrLoadEntitiesAsync(targetRelationshipIds, AzureDigitalTwinCache.TwinCache.Relationships, x => x.IsLoaded(),
            x => AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceRelationship(x),
            _azureDigitalTwinReader.GetRelationshipsAsync);
    }

    private static async Task<IEnumerable<T>> GetOrLoadEntitiesAsync<T>(IEnumerable<string> targetIds,
        ConcurrentDictionary<string, T> cache,
        Func<T, bool> isLoaded,
        Action<T> addToCache,
        Func<IEnumerable<string>, Task<IEnumerable<T>>> getEntities)
    {
        var result = new ConcurrentBag<T>();

        var targetToLoad = targetIds.Where(x => cache.ContainsKey(x) && !isLoaded(cache[x])).ToList();

        if (targetToLoad.Count > 0)
        {
            var loadedEntities = await getEntities(targetToLoad);

            Parallel.ForEach(loadedEntities, x =>
            {
                addToCache(x);
            });
        }

        return targetIds.Select(x => cache[x]);
    }

    public async override Task<BasicDigitalTwin> GetDigitalTwinAsync(string twinId)
    {
        if (!AzureDigitalTwinCache.TwinCache.Twins.TryGetValue(twinId, out var twinVal))
        {
            return null;
        }

        if (twinVal != null)
        {
            return AzureDigitalTwinCache.TwinCache.Twins[twinId];
        }

        var twin = await _azureDigitalTwinReader.GetDigitalTwinAsync(twinId);

        if (twin == null)
        {
            return null;
        }

        AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceTwin(twin);

        return twin;
    }

    public async override Task<Model.Adt.Page<BasicRelationship>> GetRelationshipsAsync(string continuationToken = null)
    {
        var targetIds = AzureDigitalTwinCache.TwinCache.Relationships.Select(x => x.Key).ToPageModel(GetPageNumber(continuationToken), DefaultPageSize);

        var relationships = await GetRelationshipsAsync(targetIds.Content);

        return Page<BasicRelationship>.FromValues(relationships.ToList().AsReadOnly(), targetIds.ContinuationToken, null).ToPageModels();
    }

    public async override Task<BasicRelationship> GetRelationshipAsync(string relationshipId, string twinId)
    {
        if (!AzureDigitalTwinCache.TwinCache.Relationships.TryGetValue(relationshipId, out var relation))
        {
            return null;
        }

        if (relation.IsLoaded())
        {
            return AzureDigitalTwinCache.TwinCache.Relationships[relationshipId];
        }

        var relationship = await _azureDigitalTwinReader.GetRelationshipAsync(relationshipId, twinId);

        if (relationship == null)
        {
            return null;
        }

        AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceRelationship(relationship);

        return relationship;
    }

    public override AsyncPageable<T> QueryAsync<T>(string query)
    {
        return _azureDigitalTwinReader.QueryAsync<T>(query);
    }

    public async override Task<Model.Adt.Page<BasicDigitalTwin>> QueryTwinsAsync(string query, int pageSize = DefaultPageSize, string continuationToken = null)
    {
        return await _azureDigitalTwinReader.QueryTwinsAsync(query, pageSize, continuationToken);
    }

}

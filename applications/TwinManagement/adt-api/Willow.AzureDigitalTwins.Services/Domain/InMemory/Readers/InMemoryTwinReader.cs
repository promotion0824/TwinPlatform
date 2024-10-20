using Azure;
using Azure.DigitalTwins.Core;
using DTDLParser.Models;
using System.Collections.Concurrent;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Services.Domain.InMemory.Readers;

public class InMemoryTwinReader : IAzureDigitalTwinReader
{
    protected IAzureDigitalTwinModelParser AzureDigitalTwinModelParser { get; }

    protected IAzureDigitalTwinCacheProvider AzureDigitalTwinCacheProvider { get; }

    protected const int DefaultPageSize = 100;

    protected IAzureDigitalTwinCache AzureDigitalTwinCache => AzureDigitalTwinCacheProvider.GetOrCreateCache();

    public InMemoryTwinReader(IAzureDigitalTwinModelParser azureDigitalTwinModelParser, IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider)
    {
        AzureDigitalTwinModelParser = azureDigitalTwinModelParser;
        AzureDigitalTwinCacheProvider = azureDigitalTwinCacheProvider;
    }

    public virtual Task<BasicDigitalTwin> GetDigitalTwinAsync(string twinId)
    {
        AzureDigitalTwinCache.TwinCache.Twins.TryGetValue(twinId, out BasicDigitalTwin twin);
        return Task.FromResult(twin);
    }

    public virtual Task<DigitalTwinsModelBasicData> GetModelAsync(string modelId)
    {
        if (AzureDigitalTwinCache.ModelCache.ModelInfos.TryGetValue(modelId, out (DTInterfaceInfo, DateTimeOffset?) value))
            return Task.FromResult(value.ToModelBasicData());
        return Task.FromResult<DigitalTwinsModelBasicData>(null);
    }

    public virtual Task<IEnumerable<DigitalTwinsModelBasicData>> GetModelsAsync(string rootModelId = null)
    {
        if (!string.IsNullOrEmpty(rootModelId))
        {
            var interfaceDescendants = AzureDigitalTwinModelParser.GetInterfaceDescendants(new List<string> { rootModelId });
            return Task.FromResult(AzureDigitalTwinCache.ModelCache.ModelInfos.Where(x => interfaceDescendants.ContainsKey(x.Key)).Select(x => x.Value.ToModelBasicData()));
        }

        return Task.FromResult(AzureDigitalTwinCache.ModelCache.ModelInfos.Select(x => x.Value.ToModelBasicData()));
    }

    public virtual Task<BasicRelationship> GetRelationshipAsync(string relationshipId, string twinId)
    {
        AzureDigitalTwinCache.TwinCache.Relationships.TryGetValue(relationshipId, out BasicRelationship relationship);
        return Task.FromResult(relationship);
    }

    public virtual Task<IEnumerable<BasicRelationship>> GetTwinRelationshipsAsync(string twinId, string relationshipName = null)
    {
        if (!AzureDigitalTwinCache.TwinCache.TwinRelationships.TryGetValue(twinId, out var relations))
        {
            return Task.FromResult(Enumerable.Empty<BasicRelationship>());
        }

        return Task.FromResult<IEnumerable<BasicRelationship>>(relations.Select(x => AzureDigitalTwinCache.TwinCache.Relationships[x]).Where(x => relationshipName == null || x.Name == relationshipName).ToList());
    }

    public virtual Task<IEnumerable<BasicRelationship>> GetRelationshipsAsync(IEnumerable<string> ids = null)
    {
        if (ids != null && ids.Any())
            return Task.FromResult(AzureDigitalTwinCache.TwinCache.Relationships.Where(x => ids.Contains(x.Key)).Select(x => x.Value));

        return Task.FromResult(AzureDigitalTwinCache.TwinCache.Relationships.Select(x => x.Value));
    }

    public virtual Task<int> GetRelationshipsCountAsync()
    {
        return Task.FromResult(AzureDigitalTwinCache.TwinCache.Relationships.Count);
    }

    public virtual Task<int> GetTwinsCountAsync()
    {
        return Task.FromResult(AzureDigitalTwinCache.TwinCache.Twins.Count);
    }

    public virtual Task<IEnumerable<BasicRelationship>> GetIncomingRelationshipsAsync(string twinId)
    {
        if (!AzureDigitalTwinCache.TwinCache.TwinIncomingRelationships.TryGetValue(twinId, out var relations))
        {
            return Task.FromResult(Enumerable.Empty<BasicRelationship>());
        }

        return Task.FromResult<IEnumerable<BasicRelationship>>(relations.Select(x => AzureDigitalTwinCache.TwinCache.Relationships[x]).ToList());
    }

    public virtual Task<Model.Adt.Page<BasicDigitalTwin>> QueryTwinsAsync(string query, int pageSize = DefaultPageSize, string continuationToken = null)
    {
        if (query != null && query.Equals("select * from digitaltwins", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AzureDigitalTwinCache.TwinCache.Twins.Select(x => x.Value).ToPageModel(GetPageNumber(continuationToken), pageSize));
        }

        throw new Exception("Disconnected in memory reader does not support queries other than 'select * from digitaltwins'");
    }

    public virtual async Task<IEnumerable<(BasicDigitalTwin, IEnumerable<BasicRelationship>, IEnumerable<BasicRelationship>)>> AppendRelationships(IEnumerable<BasicDigitalTwin> twins, bool includeRelationships, bool includeIncomingRelationships)
    {
        if (!includeIncomingRelationships && !includeRelationships)
            return twins.Select(x => (x, Enumerable.Empty<BasicRelationship>(), Enumerable.Empty<BasicRelationship>()));

        var resultTwins = new ConcurrentBag<(BasicDigitalTwin, IEnumerable<BasicRelationship>, IEnumerable<BasicRelationship>)>();
        var loadRelationships = twins.Select(async x =>
        {

            var sourceRelationships = new List<BasicRelationship>();
            if (includeRelationships)
                sourceRelationships.AddRange(await GetTwinRelationshipsAsync(x.Id));

            var targetRelationships = new List<BasicRelationship>();
            if (includeIncomingRelationships)
                targetRelationships.AddRange(await GetIncomingRelationshipsAsync(x.Id));

            resultTwins.Add((x, sourceRelationships, targetRelationships));
        });

        await Task.WhenAll(loadRelationships);

        return resultTwins;
    }

    public virtual async Task<Model.Adt.Page<BasicDigitalTwin>> GetTwinsAsync(
                                                                        GetTwinsInfoRequest request = null,
                                                                        IEnumerable<string> twinIds = null,
                                                                        int pageSize = DefaultPageSize,
                                                                    bool includeCountQuery = false,
                                                                        string continuationToken = null)
    {
        var targetTwinIds = Enumerable.Empty<string>();
        if (twinIds?.Any() == true || request?.ModelId?.Length > 0)
        {
            targetTwinIds = twinIds;

            if (request?.ModelId != null && request.ModelId.Length > 0)
            {
                var applicableModels = request?.ExactModelMatch == true ? request.ModelId.ToList() : AzureDigitalTwinModelParser.GetInterfaceDescendants(request.ModelId).Select(x => x.Key).ToList();
                var targetModelTwinIds = AzureDigitalTwinCache.TwinCache.TwinsByModel.Where(x => applicableModels.Contains(x.Key)).SelectMany(x => x.Value).ToList();

                targetTwinIds = targetTwinIds != null && targetTwinIds.Any() ? targetModelTwinIds.Intersect(targetTwinIds).ToList() : targetModelTwinIds;
            }
        }
        else
            targetTwinIds = AzureDigitalTwinCache.TwinCache.Twins.Select(x => x.Key).ToList();

        var pageTargetIds = targetTwinIds.ToPageModel(GetPageNumber(continuationToken), pageSize);

        var twins = await GetTwinsFromTargetIdsAsync(pageTargetIds.Content);

        if (!string.IsNullOrWhiteSpace(request?.SearchString))
        {
            // TODO: If we use memory reader, handle searching Name field
            twins = twins.Where(t => t.Id.Contains(request.SearchString, StringComparison.InvariantCultureIgnoreCase));
        }

        return Azure.Page<BasicDigitalTwin>.FromValues(twins.ToList().AsReadOnly(), pageTargetIds.ContinuationToken, null).ToPageModels();
    }

    protected virtual Task<IEnumerable<BasicDigitalTwin>> GetTwinsFromTargetIdsAsync(IEnumerable<string> targetTwinIds)
    {
        return Task.FromResult<IEnumerable<BasicDigitalTwin>>(targetTwinIds.Where(x => AzureDigitalTwinCache.TwinCache.Twins.ContainsKey(x)).Select(x => AzureDigitalTwinCache.TwinCache.Twins[x]).ToList());
    }

    public virtual AsyncPageable<T> QueryAsync<T>(string query)
    {
        throw new NotImplementedException("Disconnected in memory reader does not support query");
    }

    public bool IsServiceReady()
    {
        return AzureDigitalTwinCacheProvider.IsCacheReady();
    }

    protected static int GetPageNumber(string continuationToken)
    {
        if (string.IsNullOrEmpty(continuationToken) || !int.TryParse(continuationToken, out int pageNumber))
            return 1;

        return pageNumber;
    }

    public virtual Task<Model.Adt.Page<BasicRelationship>> GetRelationshipsAsync(string continuationToken = null)
    {
        return Task.FromResult(AzureDigitalTwinCache.TwinCache.Relationships.Select(x => x.Value).ToPageModel(GetPageNumber(continuationToken), DefaultPageSize));
    }

    public Task<Model.Adt.Page<BasicDigitalTwin>> QueryTwinsAsync(string query, int pageSize = 100, string continuationToken = null, string countQuery = null)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTwinsCountAsyncWithSearch(GetTwinsInfoRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Model.Adt.Page<BasicDigitalTwin>> GetTwinsByIdsAsync(IEnumerable<string> twinIds = null)
    {
        throw new NotImplementedException();
    }
}

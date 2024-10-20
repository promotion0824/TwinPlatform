using Azure;
using Azure.Core;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Polly;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Willow.AzureDigitalTwins.Services.Builders;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Services.Domain.Instance.Readers;


public partial class AzureDigitalTwinReader : IAzureDigitalTwinReader
{
    private readonly DigitalTwinsClient _digitalTwinsClient;
    private const string twinCollectionName = "twins";
    private readonly ILogger<AzureDigitalTwinReader> _logger;
    private Random _rnd = new Random();

    [GeneratedRegex("SELECT\\s+(.+?)\\s+FROM", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AdtQueryPatternRegEx();

    public AzureDigitalTwinReader(InstanceSettings settings, TokenCredential tokenCredential, ILogger<AzureDigitalTwinReader> logger)
    {
        _digitalTwinsClient = new DigitalTwinsClient(settings.InstanceUri, tokenCredential);
        _logger = logger;
    }

    public AsyncPageable<BasicDigitalTwin> GetPageableTwins()
    {
        return QueryAsync<BasicDigitalTwin>(QueryBuilder.Create().SelectAll().FromDigitalTwins().GetQuery());
    }

    public AsyncPageable<BasicRelationship> GetPageableRelationships()
    {
        return QueryAsync<BasicRelationship>(QueryBuilder.Create().SelectAll().FromRelationships().GetQuery());
    }

    public async Task<int> GetTwinsCountAsync()
    {
        return await GetCountAsync(QueryBuilder.Create().SelectCount().FromDigitalTwins().GetQuery());
    }

    public async Task<int> GetRelationshipsCountAsync()
    {
        return await GetCountAsync(QueryBuilder.Create().SelectCount().FromRelationships().GetQuery());
    }

    private async Task<int> GetCountAsync(string query)
    {
        var queryResult = QueryAsync<CountResult>(query);
        _logger.LogInformation("GetCountAsync query: {Query}", query);

        var page = await queryResult.AsPages().SingleAsync();

        return page.Values.Single().COUNT;
    }

    //Sample ADT query with filters(Model: "Sensor", Location: "International Maritime Innovation Center", Search string:"faw")
    //with start and end search update time of destination twin applied generates the below query:
    //select twins from DIGITALTWINS match(twins)-[:isPartOf|locatedIn|hostedBy|isCapabilityOf*..6]->(location) where location.$dtId = 'FAW-IMIC'
    //AND(IS_OF_MODEL(twins, 'dtmi:com:willowinc:Sensor;1')) AND(IS_OF_MODEL(location, 'dtmi:com:willowinc:Space;1') OR
    //IS_OF_MODEL(location, 'dtmi:com:willowinc:Collection;1')) AND(IS_OF_MODEL(twins, 'dtmi:com:willowinc:Sensor;1'))
    //AND(contains(twins.$dtId, 'wil') OR contains(twins.name, 'wil') OR contains(twins.$dtId, 'Wil') OR contains(twins.name, 'Wil')
    //OR contains(twins.$dtId, 'wil') OR contains(twins.name, 'wil') OR contains(twins.$dtId, 'WIL') OR contains(twins.name, 'WIL') )
    //AND location.$metadata.$lastUpdateTime >= '2023-06-04T20:37:21Z' and location.$metadata.$lastUpdateTime <= '2023-07-25T20:37:21Z'

    public virtual async Task<Model.Adt.Page<BasicDigitalTwin>> GetTwinsAsync(
        GetTwinsInfoRequest request = null,
        IEnumerable<string> twinIds = null,
        int pageSize = 100,
        bool includeCountQuery = false,
        string continuationToken = null)
    {
        var queryBuilder = QueryBuilder.Create().Select(twinCollectionName).FromDigitalTwins(twinCollectionName);
        if (request == null)
            request = new GetTwinsInfoRequest();
        queryBuilder = (IQueryWhere)QueryBuilder.BuildTwinsQuery(queryBuilder, twinIds, modelIds: request?.ModelId, request?.LocationId,
            request?.RelationshipsToTraverse, request?.SearchString, request is not null && request.ExactModelMatch, request?.StartTime,
            request?.EndTime, request.QueryFilter?.Filter);

        var countQueryBuilder = QueryBuilder.Create().SelectCount().FromDigitalTwins(twinCollectionName);
        countQueryBuilder = (IQueryWhere)QueryBuilder.BuildTwinsQuery(countQueryBuilder, twinIds, modelIds: request?.ModelId, request?.LocationId,
            relationshipToTraverse: request?.RelationshipsToTraverse, request?.SearchString, request is not null && request.ExactModelMatch,
            request?.StartTime, request?.EndTime, request.QueryFilter?.Filter, isCountQuery: true);

        _logger.LogTrace("GetTwinsAsync GetTwinsInfoRequest: {Request} ", JsonSerializer.Serialize(request));
        _logger.LogInformation("GetTwinsAsync query (pageSize:{PageSize}): {Query}", pageSize, queryBuilder.GetQuery());

        return includeCountQuery ? await QueryTwinsAsync(queryBuilder.GetQuery(), pageSize, continuationToken, countQueryBuilder.GetQuery()) :
                        await QueryTwinsAsync(queryBuilder.GetQuery(), pageSize, continuationToken);
    }

    public async Task<Model.Adt.Page<BasicDigitalTwin>> GetTwinsByIdsAsync(IEnumerable<string> twinIds = null)
    {
        var queryBuilder = QueryBuilder.Create().Select(twinCollectionName).FromDigitalTwins(twinCollectionName);
        queryBuilder = (IQueryWhere)QueryBuilder.BuildTwinsQuery(queryBuilder, twinIds);

        _logger.LogInformation("GetTwinsByIdsAsync query: {Query}", queryBuilder.GetQuery());

        // In QueryTwinsAsync, AsPages() is not working properly to find all the twins
        // even though they are present in the ADT. So, we are using QueryTwinsByIdsAsync without paging for now.
        // This issue found on CI environment only at this time.
        // Will update this once we find the root cause.
        //return await QueryTwinsAsync(queryBuilder.GetQuery(), pageSize, continuationToken);
        return await QueryTwinsByIdsAsync(queryBuilder.GetQuery());
    }

    public async Task<int> GetTwinsCountAsyncWithSearch(GetTwinsInfoRequest request = null)
    {
        var countQueryBuilder = QueryBuilder.Create().SelectCount().FromDigitalTwins();
        countQueryBuilder = (IQueryWhere)QueryBuilder.BuildTwinsQuery(countQueryBuilder, twinIds: null, modelIds: request.ModelId, request.LocationId, request.RelationshipsToTraverse, request.SearchString, modelExactMatch: request.ExactModelMatch, request.StartTime, request.EndTime);

        _logger.LogInformation("GetTwinsCountAsyncWithSearch query: {Query}", countQueryBuilder.GetQuery());

        return await GetCountAsync(countQueryBuilder.GetQuery());
    }

    public async Task<Model.Adt.Page<BasicDigitalTwin>> QueryTwinsByIdsAsync(string query)
    {
        var queryResults = QueryAsync<JsonDocument>(query);

        var twins = await queryResults.ToListAsync();

        _logger.LogInformation("QueryTwinsByIdsAsync count: {Count}", twins.Count);

        return twins.ExtractToPageModel<BasicDigitalTwin>(PullSelectPropertyName(query));
    }

    public async Task<Model.Adt.Page<BasicDigitalTwin>> QueryTwinsAsync(string query, int pageSize = 100, string continuationToken = null)
    {
        // We deserialize as generic JsonDocument to accomodate query response having different structures
        // 1. Select * from DigitalTwins
        // 2. Select twins from DigitalTwins twins
        var queryResults = QueryAsync<JsonDocument>(query);

        var page = await queryResults.AsPages(continuationToken, pageSizeHint: pageSize).FirstOrDefaultAsync();

        _logger.LogInformation("QueryTwinsAsync WITHOUT countQuery:{Count}", page.Values.Count);

        return page.ExtractToPageModel<BasicDigitalTwin>(PullSelectPropertyName(query));
    }

    public async Task<Model.Adt.Page<BasicDigitalTwin>> QueryTwinsAsync(string query, int pageSize = 100, string continuationToken = null, string countQuery = null)
    {
        var queryResults = QueryAsync<JsonDocument>(query);

        // first fetch
        if (string.IsNullOrEmpty(continuationToken))
        {
            // Note: If we pass in the continuation token here (even though it's null) and the pageSizeHint:pageSize 
            //   we appear to only get a sub-set of twins returned (less than page size) with no continuation token

            var totalTwins = await GetCountAsync(countQuery);

            int? pageSizeHint = totalTwins < pageSize ? (int?)null : pageSize;

            var page = await queryResults.AsPages(pageSizeHint: pageSizeHint).FirstOrDefaultAsync();
          
            var newPageQueryResult = new PageQueryResult
            {

                NextPage = 0,
                Total = totalTwins,
                continuationtoken = page.ContinuationToken,
            };
            _logger.LogInformation("QueryTwinsAsync with countQuery first fetch row count: {Count}", page.Values.Count);
            var ctToken = JsonSerializer.Serialize(newPageQueryResult);
           return page.ExtractToPageModel<BasicDigitalTwin>(PullSelectPropertyName(query), ctToken);
        }
        // subsequent fetch
        else
        {
            var pageQueryResult = JsonSerializer.Deserialize<PageQueryResult>(continuationToken);

            var page = await queryResults.AsPages(pageQueryResult.continuationtoken, pageSize).FirstOrDefaultAsync();

            var newPageQueryResult = new PageQueryResult
            {
                NextPage = pageQueryResult.NextPage,
                Total = pageQueryResult.Total,
                continuationtoken = page.ContinuationToken,
            };
            _logger.LogInformation("QueryTwinsAsync with countQuery subsequent fetch row count: {Count}", page.Values.Count);

            var ctToken = JsonSerializer.Serialize(newPageQueryResult);
            return page.ExtractToPageModel<BasicDigitalTwin>(PullSelectPropertyName(query), ctToken);
        }
    }

    public class PageQueryResult
    {
        public int NextPage { get; set; }
        public int Total { get; set; }
        public string continuationtoken { get; set; }

    }

    public async Task<IEnumerable<(BasicDigitalTwin, IEnumerable<BasicRelationship>, IEnumerable<BasicRelationship>)>> AppendRelationships(IEnumerable<BasicDigitalTwin> twins, bool includeRelationships, bool includeIncomingRelationships)
    {
        if (!includeIncomingRelationships && !includeRelationships)
            return twins.Select(x => (x, Enumerable.Empty<BasicRelationship>(), Enumerable.Empty<BasicRelationship>()));

        var twinIds = twins.Select(x => x.Id).ToList();

        var sourceRelationships = new List<BasicRelationship>();
        var targetRelationships = new List<BasicRelationship>();

        var loadOutgoing = async () =>
        {
            var rel = await GetSourceRelationshipsAsync(twinIds, includeRelationships);
            sourceRelationships.AddRange(rel);
        };

        var loadIncoming = async () =>
        {
            var rel = await GetTargetRelationshipsAsync(twinIds, includeIncomingRelationships);
            targetRelationships.AddRange(rel);
        };

        await Task.WhenAll(loadIncoming(), loadOutgoing());

        return twins.Select(x => (x, sourceRelationships.Where(r => r.SourceId == x.Id), targetRelationships.Where(r => r.TargetId == x.Id)));
    }

    protected virtual async Task<IEnumerable<BasicRelationship>> GetSourceRelationshipsAsync(IEnumerable<string> twinIds, bool include)
    {
        return await GetTwinRelationshipsAsync(include, twinIds, QueryBuilder.FieldRelationshipSourceId);
    }

    protected virtual async Task<IEnumerable<BasicRelationship>> GetTargetRelationshipsAsync(IEnumerable<string> twinIds, bool include)
    {
        return await GetTwinRelationshipsAsync(include, twinIds, QueryBuilder.FieldRelationshipTargetId);
    }

    protected async Task<IEnumerable<BasicRelationship>> GetTwinRelationshipsAsync(bool include, IEnumerable<string> fieldValues, string filterField)
    {
        var relationships = new ConcurrentBag<BasicRelationship>();
        if (!include)
            return relationships;

        var splitValues = fieldValues.Chunk(50);

        var tasks = splitValues.Select(async x =>
        {
            var queryResults = QueryAsync<BasicRelationship>(QueryBuilder.Create().SelectAll().FromRelationships().Where().WithPropertyIn(filterField, x).GetQuery());

            var pages = await queryResults.AsPages().ToListAsync();
            pages.SelectMany(x => x.Values)
                    .Where(x => x != null)
                    .ToList()
                    .ForEach(x => relationships.Add(x));
        });

        await Task.WhenAll(tasks);

        return relationships;
    }

    public virtual async Task<BasicDigitalTwin> GetDigitalTwinAsync(string twinId)
    {
        var response = await _digitalTwinsClient.GetDigitalTwinAsync<BasicDigitalTwin>(twinId);
        return response.Value;
    }

    public virtual async Task<IEnumerable<BasicRelationship>> GetTwinRelationshipsAsync(string twinId, string relationshipName = null)
    {
        var response = _digitalTwinsClient.GetRelationshipsAsync<BasicRelationship>(twinId, relationshipName);

        var pages = await response.AsPages().ToListAsync();

        return pages.SelectMany(x => x.Values);
    }

    public virtual async Task<IEnumerable<BasicRelationship>> GetIncomingRelationshipsAsync(string twinId)
    {
        var response = _digitalTwinsClient.GetIncomingRelationshipsAsync(twinId);
        var pages = await response.AsPages().ToListAsync();

        return pages.SelectMany(x => x.Values.Select(x => new BasicRelationship { Id = x.RelationshipId, SourceId = x.SourceId, TargetId = twinId, Name = x.RelationshipName }));
    }

    public virtual async Task<IEnumerable<BasicRelationship>> GetRelationshipsAsync(IEnumerable<string> ids = null)
    {
        if (ids != null && ids.Any())
            return await GetTwinRelationshipsAsync(true, ids, QueryBuilder.FieldRelationshipId);

        var pages = await GetPageableRelationships().AsPages().ToListAsync();
        return pages.SelectMany(x => x.Values);
    }

    public async Task<Model.Adt.Page<BasicRelationship>> GetRelationshipsAsync(string continuationToken = null)
    {
        var page = await GetPageableRelationships().AsPages(continuationToken).FirstOrDefaultAsync();
        return page.ToPageModels();
    }

    public virtual async Task<BasicRelationship> GetRelationshipAsync(string relationshipId, string twinId)
    {
        var response = await _digitalTwinsClient.GetRelationshipAsync<BasicRelationship>(twinId, relationshipId);
        return response.Value;
    }

    public virtual async Task<IEnumerable<DigitalTwinsModelBasicData>> GetModelsAsync(string rootModelId = null)
    {
        var modelOptions = new GetModelsOptions { IncludeModelDefinition = true };
        if (!string.IsNullOrEmpty(rootModelId))
            modelOptions.DependenciesFor = new List<string> { rootModelId };

        var response = _digitalTwinsClient.GetModelsAsync(modelOptions);

        var pages = await response.AsPages().ToListAsync();

        return pages.SelectMany(x => x.Values).Where(x => !x.Decommissioned.Value).Select(x =>
            new DigitalTwinsModelBasicData
            {
                Id = x.Id,
                DtdlModel = x.DtdlModel,
                UploadedOn = x.UploadedOn
            });
    }

    public virtual async Task<DigitalTwinsModelBasicData> GetModelAsync(string modelId)
    {
        var response = await _digitalTwinsClient.GetModelAsync(modelId);
        return new DigitalTwinsModelBasicData { Id = response.Value.Id, DtdlModel = response.Value.DtdlModel, UploadedOn = response.Value.UploadedOn };
    }

    public AsyncPageable<T> QueryAsync<T>(string query)
    {
        AsyncPageable<T> response = null;

        // Note that the MS SDK does its own (4) quick retries which we can't disable --
        //   so each retry here can result in 4 actual requests. We'll start with longer delays because of this.
        //   (+/- 4, 8, 16, 32, 64 seconds)
        // Another option is to use do throttled reads (delaying requests)  based on the ADT limits.
        using (_logger.BeginScope(new Dictionary<string, string> { ["Query"] = query }))
        {
            var numCalls = 0;
            try
            {
                // NOTE: We could use a hash of the query to identify equal queries in the logs w/o leaking any details

                // Exponential backoff up to 64 seconds with 25% jitter
                var retryPolicy = Policy
                    .Handle<RequestFailedException>(x => x.Status == (int)HttpStatusCode.TooManyRequests)
                    .WaitAndRetry(5, i =>
                    {
                        var ms = 1000 * (1 << i + 1);
                        var jitter = _rnd.Next(-ms / 8, ms / 8);
                        return TimeSpan.FromMilliseconds(ms + jitter);
                    });

                retryPolicy.Execute(() =>
                {
                    ++numCalls;
                    response = _digitalTwinsClient.QueryAsync<T>(query);
                });

                if (numCalls > 1)
                    _logger.LogWarning("QueryAsync: Succeeded after {NumCalls} calls", numCalls);
                else
                    _logger.LogTrace("QueryAsync: Success"); // Note: query captured in log scope
                                                             // TODO: output custom telemetry for numCalls

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueryAsync: Failed after {NumCalls} calls", numCalls);
                throw;
            }
        }
    }

    public virtual bool IsServiceReady()
    {
        return true;
    }

    /// <summary>
    /// Extract the projected property name from the ADT Query
    /// </summary>
    /// <param name="adtQuery">Input ADT Query</param>
    /// <returns>Name of the projected column</returns>
    /// <exception cref="InvalidDataException"></exception>
    private string PullSelectPropertyName(string adtQuery)
    {
        Match match = AdtQueryPatternRegEx().Match(adtQuery);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        else
        {
            _logger.LogError("ADT Query {Query} is not in the right pattern", adtQuery);
            throw new InvalidDataException();
        }
    }
}

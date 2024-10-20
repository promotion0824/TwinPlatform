using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using DigitalTwinCore.Extensions;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services.Cacheless;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Willow.Infrastructure.Exceptions;
using Willow.Logging;
using JsonPatchDocument = Microsoft.AspNetCore.JsonPatch.JsonPatchDocument;

namespace DigitalTwinCore.Services.AdtApi
{
    public interface IAdtApiService
    {
        List<AdtModel> GetModels(AzureDigitalTwinsSettings instanceSettings);
        Task<AdtModel> GetModel(AzureDigitalTwinsSettings instanceSettings, string modelId);
        Task<AdtModel> CreateModel(AzureDigitalTwinsSettings instanceSettings, string modelJson);
        Task DeleteModel(AzureDigitalTwinsSettings instanceSettings, string modelId);

        Task<BasicDigitalTwin> GetTwin(AzureDigitalTwinsSettings instanceSettings, string twinId);
        Task<List<BasicDigitalTwin>> GetTwins(AzureDigitalTwinsSettings instanceSettings, string query = null);
        Task<BasicDigitalTwin> AddOrUpdateTwin(AzureDigitalTwinsSettings instanceSettings, string twinId, BasicDigitalTwin twin);
        Task PatchTwin(AzureDigitalTwinsSettings instanceSettings, string twinId, JsonPatchDocument patch, Azure.ETag? ifMatch);

        Task<BasicRelationship> AddRelationship(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipId, BasicRelationship relationship);
        Task<List<BasicRelationship>> GetRelationships(AzureDigitalTwinsSettings instanceSettings, string twinId);
        Task<List<IncomingRelationship>> GetIncomingRelationships(AzureDigitalTwinsSettings instanceSettings, string twinId);
        Task DeleteRelationship(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipId);
        Task<BasicRelationship> GetRelationship(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipId);
        Task<BasicRelationship> UpdateRelationship(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipId, JsonPatchDocument patchJson);

        Task DeleteTwin(AzureDigitalTwinsSettings instanceSettings, string twinId);
        Task<List<JsonElement>> QueryTwins(AzureDigitalTwinsSettings instanceSettings, string sql);
        Azure.AsyncPageable<T> QueryTwins<T>(AzureDigitalTwinsSettings instanceSettings, string sql);
        Dictionary<string, List<string>> GetLatestExecutedQueries();

        Task<Models.Page<BasicDigitalTwin>> GetTwinsAsync(
            AzureDigitalTwinsSettings instanceSettings,
            GetTwinsInfoRequest request = null,
            IEnumerable<string> twinIds = null,
            int pageSize = 100,
            bool includeCountQuery = false,
            string continuationToken = null);
        Task<IEnumerable<BasicRelationship>> GetRelationshipsAsync(AzureDigitalTwinsSettings instanceSettings, string twinId,string relationshipName = null);
        Task<IEnumerable<BasicRelationship>> GetIncomingRelationshipsAsync(AzureDigitalTwinsSettings instanceSettings, string twinId);
    }

    //TODO: Add cache to save some API calls
    public partial class AdtApiService : IAdtApiService
    {
        private const string twinCollectionName = "twins";
        private Random _rnd = new Random();
        private readonly ITokenService _tokenService;
        private readonly ILogger<AdtApiService> _logger;
        private static Dictionary<string, List<string>> _executedQueries = new Dictionary<string, List<string>>();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;

        [GeneratedRegex("SELECT\\s+(.+?)\\s+FROM", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex AdtQueryPatternRegEx();

        public AdtApiService(ITokenService tokenService, ILogger<AdtApiService> logger, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _tokenService = tokenService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Gets or creates a cached instance of the Azure Digital Twins client. Caching is
        /// implemented to avoid unnecessary token generation and DefaultAzureCredential instantiation
        /// in case of multiple calls to the GetClient method within a specified time window. The
        /// caching key is based on the unique instance URI. Example use cases include retrieving a
        /// list of sites based on the specified scope or getting twins and their children in tree form.
        /// </summary>
        /// <param name="instanceSettings">Settings for the Azure Digital Twins instance.</param>
        /// <returns>An instance of DigitalTwinsClient associated with the specified Azure Digital Twins instance.</returns>
        private DigitalTwinsClient GetClient(AzureDigitalTwinsSettings instanceSettings)
        {
            var digitalTwinsClient = _memoryCache.GetOrCreate($"AdtClientCache_{instanceSettings.InstanceUri}", (c) =>
            {
                // Set an absolute expiration time for the cached client to avoid using stale credentials.
                // Note: The default token expiration is at least an hour.
                c.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(55);

                return new DigitalTwinsClient(instanceSettings.InstanceUri, new DefaultAzureCredential());
            });

            return digitalTwinsClient;
        }

        public List<AdtModel> GetModels(AzureDigitalTwinsSettings instanceSettings)
        {
            try
            {
                return AdtModel.MapFrom(GetClient(instanceSettings).GetModels(new GetModelsOptions { IncludeModelDefinition = true }));
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex);
            }
        }

        public async Task<AdtModel> GetModel(AzureDigitalTwinsSettings instanceSettings, string modelId)
        {
            try
            {
                return AdtModel.MapFrom(await GetClient(instanceSettings).GetModelAsync(modelId));
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] { new KeyValuePair<string, object>(nameof(modelId), modelId) });
            }
        }

        public async Task<AdtModel> CreateModel(AzureDigitalTwinsSettings instanceSettings, string modelJson)
        {
            try
            {
                var modelData = await GetClient(instanceSettings).CreateModelsAsync(new[] { modelJson });
                return AdtModel.MapFrom(modelData.Value.Single());
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] { new KeyValuePair<string, object>(nameof(modelJson), modelJson) });
            }
        }

        public async Task DeleteModel(AzureDigitalTwinsSettings instanceSettings, string modelId)
        {
            try
            {
                await GetClient(instanceSettings).DeleteModelAsync(modelId);
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] { new KeyValuePair<string, object>(nameof(modelId), modelId) });
            }
        }

        public async Task<BasicDigitalTwin> GetTwin(AzureDigitalTwinsSettings instanceSettings, string twinId)
        {
            try
            {
                var response = await GetClient(instanceSettings).GetDigitalTwinAsync<BasicDigitalTwin>(twinId);
                return response.Value;
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] { new KeyValuePair<string, object>(nameof(twinId), twinId) });
            }
        }

        public async Task<BasicDigitalTwin> AddOrUpdateTwin(AzureDigitalTwinsSettings instanceSettings, string twinId, BasicDigitalTwin twin)
        {
            try
            {
                var response = await GetClient(instanceSettings).CreateOrReplaceDigitalTwinAsync(twinId, twin);
                return response.Value;
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] {
                    new KeyValuePair<string, object>(nameof(twinId), twinId),
                    new KeyValuePair<string, object>(nameof(twin), twin)});
            }
        }

        public async Task PatchTwin(AzureDigitalTwinsSettings instanceSettings, string twinId, JsonPatchDocument patch, Azure.ETag? ifMatch)
        {
            try
            {
                var response = await GetClient(instanceSettings).UpdateDigitalTwinAsync(twinId, patch.ConvertToAzureJsonPatchDocument(), ifMatch);
                response.EnsureSuccessStatusCode("DigitalTwinCore:Livedata");
            }
            catch (Azure.RequestFailedException ex)
            {
                if (ex.Status == (int)HttpStatusCode.PreconditionFailed)
                {
                    throw new PreconditionFailedException("Precondition failed");
                }

                throw HandleRequestFailed(instanceSettings, ex, new[] {
                    new KeyValuePair<string, object>(nameof(twinId), twinId),
                    new KeyValuePair<string, object>(nameof(patch), patch)},
                    "PatchTwin");
            }
        }

        public async Task<BasicRelationship> AddRelationship(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipId, BasicRelationship relationship)
        {
            try
            {
                var response = await GetClient(instanceSettings).CreateOrReplaceRelationshipAsync(twinId, relationshipId, relationship);
                return response.Value;
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] {
                    new KeyValuePair<string, object>(nameof(twinId), twinId),
                    new KeyValuePair<string, object>(nameof(relationshipId), relationshipId),
                    new KeyValuePair<string, object>(nameof(relationship), relationship)});
            }
        }

        public async Task<List<BasicRelationship>> GetRelationships(AzureDigitalTwinsSettings instanceSettings, string twinId)
        {
            try
            {
                var response = GetClient(instanceSettings).GetRelationshipsAsync<BasicRelationship>(twinId);
                return await response.ToListAsync();
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] { new KeyValuePair<string, object>(nameof(twinId), twinId) });
            }
        }

        public async Task<List<IncomingRelationship>> GetIncomingRelationships(AzureDigitalTwinsSettings instanceSettings, string twinId)
        {
            try
            {
                var response = GetClient(instanceSettings).GetIncomingRelationshipsAsync(twinId);
                return await response.ToListAsync();
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] { new KeyValuePair<string, object>(nameof(twinId), twinId) });
            }
        }

        public async Task<List<BasicDigitalTwin>> GetTwins(AzureDigitalTwinsSettings instanceSettings, string query = null)
		{
			query ??= "SELECT * FROM DIGITALTWINS";
			var results = await QueryTwins(instanceSettings, query);

            AddToExecutedQueries(query);

            return results.Select(r => JsonSerializer.Deserialize<BasicDigitalTwin>(r.GetRawText())).ToList();
		}

        private void AddToExecutedQueries(string query)
        {
            // Not needed right now.
            return;

            if (_httpContextAccessor == null || _httpContextAccessor.HttpContext == null)
                return;            
            
            var key = _httpContextAccessor.HttpContext.Request.Path;
            if (_httpContextAccessor.HttpContext.Request.QueryString.HasValue)
                key += $"?{_httpContextAccessor.HttpContext.Request.QueryString.Value}";

            if (!_executedQueries.ContainsKey(key))
                _executedQueries.Add(key, new List<string>());

            _executedQueries[key].Add(query);
        }

		public async Task DeleteTwin(AzureDigitalTwinsSettings instanceSettings, string twinId)
        {
            try
            {
                await GetClient(instanceSettings).DeleteDigitalTwinAsync(twinId);
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] { new KeyValuePair<string, object>(nameof(twinId), twinId) });
            }
        }

        public async Task<List<JsonElement>> QueryTwins(AzureDigitalTwinsSettings instanceSettings, string sql)
		{
			try
            {
                AddToExecutedQueries(sql);

                return await GetClient(instanceSettings).QueryAsync<JsonElement>(sql).ToListAsync();
			}
			catch (Exception ex)
			{
				throw HandleRequestFailed(instanceSettings, ex, new[] { new KeyValuePair<string, object>(nameof(sql), sql) });
			}
		}

        public Dictionary<string, List<string>> GetLatestExecutedQueries()
        {
            return _executedQueries;
        }

		public Azure.AsyncPageable<T> QueryTwins<T>(AzureDigitalTwinsSettings instanceSettings, string sql)
        {
            try
            {
                AddToExecutedQueries(sql);
                Azure.AsyncPageable<T> response = null;

                var retryPolicy = Policy.Handle<Azure.RequestFailedException>(x => x.Status == (int)HttpStatusCode.TooManyRequests)
                    .WaitAndRetry(5, i => TimeSpan.FromSeconds(1));

                retryPolicy.Execute(() =>
                {
                    response = GetClient(instanceSettings).QueryAsync<T>(sql);
                });

                return response;
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] { new KeyValuePair<string, object>(nameof(sql), sql) });
            }
        }

        public async Task DeleteRelationship(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipId)
        {
            try
            {
                await GetClient(instanceSettings).DeleteRelationshipAsync(twinId, relationshipId);
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] {
                    new KeyValuePair<string, object>(nameof(twinId), twinId),
                    new KeyValuePair<string, object>(nameof(relationshipId), relationshipId)});
            }
        }

        public async Task<BasicRelationship> GetRelationship(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipId)
        {
            try
            {
                var response = await GetClient(instanceSettings).GetRelationshipAsync<BasicRelationship>(twinId, relationshipId);
                return response.Value;
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] { 
                    new KeyValuePair<string, object>(nameof(twinId), twinId) ,
                    new KeyValuePair<string, object>(nameof(relationshipId), relationshipId) });
            }
        }

        public async Task<BasicRelationship> UpdateRelationship(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipId, JsonPatchDocument patchJson)
        {
            try
            {                
                await GetClient(instanceSettings).UpdateRelationshipAsync(twinId, relationshipId, patchJson.ConvertToAzureJsonPatchDocument());
                return await GetRelationship(instanceSettings, twinId, relationshipId);
            }
            catch (Exception ex)
            {
                throw HandleRequestFailed(instanceSettings, ex, new[] { 
                    new KeyValuePair<string, object>(nameof(twinId), twinId),
                    new KeyValuePair<string, object>(nameof(relationshipId), relationshipId),
                    new KeyValuePair<string, object>(nameof(patchJson), patchJson)});
            }
        }

        private AdtApiException HandleRequestFailed(AzureDigitalTwinsSettings instanceSettings, Exception innerException, KeyValuePair<string, object>[] parameters = null, [CallerMemberName] string methodName = null)
        {
            var exception = new AdtApiException(innerException, instanceSettings?.InstanceUri, methodName, parameters);
            _logger.LogError($"AdtApiService exception in {methodName ?? ""}", exception, instanceSettings);
            return exception;
        }

        public virtual async Task<IEnumerable<BasicRelationship>> GetRelationshipsAsync(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipName = null)
        {
            var response = GetClient(instanceSettings).GetRelationshipsAsync<BasicRelationship>(twinId, relationshipName);

            var pages = await response.AsPages().ToListAsync();

            return pages.SelectMany(x => x.Values);
        }

        public virtual async Task<IEnumerable<BasicRelationship>> GetIncomingRelationshipsAsync(AzureDigitalTwinsSettings instanceSettings, string twinId)
        {
            var response = GetClient(instanceSettings).GetIncomingRelationshipsAsync(twinId);
            var pages = await response.AsPages().ToListAsync();

            return pages.SelectMany(x => x.Values.Select(x => new BasicRelationship { Id = x.RelationshipId, SourceId = x.SourceId, TargetId = twinId, Name = x.RelationshipName }));
        }

        public virtual async Task<Models.Page<BasicDigitalTwin>> GetTwinsAsync(
            AzureDigitalTwinsSettings instanceSettings,
            GetTwinsInfoRequest request = null,
            IEnumerable<string> twinIds = null,
            int pageSize = 100,
            bool includeCountQuery = false,
            string continuationToken = null)
        {
            var queryBuilder = QueryBuilder.Create().Select(twinCollectionName).FromDigitalTwins(twinCollectionName);
            if (request == null)
                request = new GetTwinsInfoRequest();
            queryBuilder = (IQueryWhere)BuildTwinsQuery(queryBuilder, twinIds, modelIds: request?.ModelId, request?.LocationId, request?.RelationshipsToTraverse, request?.SearchString, request is not null && request.ExactModelMatch, request?.StartTime, request?.EndTime);

            var countQueryBuilder = QueryBuilder.Create().SelectCount().FromDigitalTwins();
            countQueryBuilder = (IQueryWhere)BuildTwinsQuery(countQueryBuilder, twinIds, modelIds: request?.ModelId, request?.LocationId, relationshipToTraverse: request?.RelationshipsToTraverse, request?.SearchString, request is not null && request.ExactModelMatch, request?.StartTime, request?.EndTime, isCountQuery: true);

            _logger.LogTrace("GetTwinsAsync GetTwinsInfoRequest: {Request} ", JsonSerializer.Serialize(request));
            _logger.LogInformation("GetTwinsAsync query (pageSize:{PageSize}): {Query}", pageSize, queryBuilder.GetQuery());

            return includeCountQuery ? await QueryTwinsAsync(instanceSettings, queryBuilder.GetQuery(), pageSize, continuationToken, countQueryBuilder.GetQuery()) :
                            await QueryTwinsAsync(instanceSettings, queryBuilder.GetQuery(), pageSize, continuationToken);
        }

        private async Task<Models.Page<BasicDigitalTwin>> QueryTwinsAsync(AzureDigitalTwinsSettings instanceSettings, string query, int pageSize = 100, string continuationToken = null)
        {
            // We deserialize as generic JsonDocument to accomodate query response having different structures
            // 1. Select * from DigitalTwins
            // 2. Select twins from DigitalTwins twins
            var queryResults = QueryAsync<JsonDocument>(instanceSettings, query);

            var page = await queryResults.AsPages(continuationToken, pageSizeHint: pageSize).FirstOrDefaultAsync();

            _logger.LogInformation("QueryTwinsAsync WITHOUT countQuery:{Count}", page.Values.Count);

            return page.ExtractToPageModel<BasicDigitalTwin>(PullSelectPropertyName(query));
        }

        private async Task<Models.Page<BasicDigitalTwin>> QueryTwinsAsync(AzureDigitalTwinsSettings instanceSettings, string query, int pageSize = 100, string continuationToken = null, string countQuery = null)
        {
            var queryResults = QueryAsync<JsonDocument>(instanceSettings, query);

            // first fetch
            if (string.IsNullOrEmpty(continuationToken))
            {

                var page = await queryResults.AsPages(continuationToken, pageSizeHint: pageSize).FirstOrDefaultAsync();
                var totalTwins = await GetCountAsync(instanceSettings, countQuery);
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

        private AsyncPageable<T> QueryAsync<T>(AzureDigitalTwinsSettings instanceSettings, string query)
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
                        response = GetClient(instanceSettings).QueryAsync<T>(query);
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

        private IQueryBuilder BuildTwinsQuery(
            IQueryBuilder query,
            IEnumerable<string> twinIds = null,
            IEnumerable<string> modelIds = null,
            string locationId = null,
            string[] relationshipToTraverse = null,
            string searchString = null,
            bool modelExactMatch = false,
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null,
            bool isCountQuery = false)
        {
            const string maxNumberOfHopsPattern = "*..6";
            bool includeAnd = false;
            const string sourceName = twinCollectionName;
            const string targetName = "location";
            var hasLocation = !string.IsNullOrEmpty(locationId);

            if (hasLocation)
            {
                var spaceCollectionModels = new List<string>() { "dtmi:com:willowinc:Space;1", "dtmi:com:willowinc:Collection;1" };
                if ((relationshipToTraverse?.Length ?? 0) == 0)
                {
                    relationshipToTraverse = new[] { "isPartOf", "locatedIn", "hostedBy", "isCapabilityOf" };
                }
                if (!isCountQuery)
                    query = QueryBuilder.Create().Select(twinCollectionName).FromDigitalTwins();

                (query as IQueryWhere).Match(
                relationshipToTraverse,
                sourceName, targetName,
                maxNumberOfHopsPattern, "-", "->");

                (query as IQueryWhere).Where().WithStringProperty($"{targetName}.$dtId", locationId);
                (query as IQueryFilterGroup).And();
                if (modelIds.Count() > 0)
                {
                    (query as IQueryWhere).Where().WithAnyModel(
                    modelIds,
                    sourceName,
                    exact: modelExactMatch);
                    (query as IQueryFilterGroup).And();
                }
            (query as IQueryWhere).Where().WithAnyModel(
            spaceCollectionModels,
            targetName,
            exact: modelExactMatch);

                includeAnd = true;
            }

            if (twinIds?.Any() == true)
            {
                (query as IQueryWhere).Where().WithPropertyIn(QueryBuilder.FieldTwinId, twinIds);
                includeAnd = true;
            }

            if (modelIds?.Any() == true)
            {
                if (includeAnd) (query as IQueryFilterGroup).And();
                (query as IQueryWhere).Where().WithAnyModel(
                                                                            modelIds,
                                                                            hasLocation ? sourceName : null,
                                                                            exact: modelExactMatch);
                includeAnd = true;
            }
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                if (includeAnd) (query as IQueryFilterGroup).And();
                if (!hasLocation) SearchQueryBuilder(searchString, query);
                else SearchQueryBuilder(searchString, query, sourceName);
            }
            if (startTime.HasValue && endTime.HasValue)
            {
                var timeSearchString = "$metadata.$lastUpdateTime";

                if (startTime > endTime) throw new InvalidDataException("StartTime is greater than EndTime");
                if (hasLocation) timeSearchString = $"{targetName}.$metadata.$lastUpdateTime";


                if (includeAnd)
                    (query as IQueryFilterGroup).And();
                else
                    (query as IQueryWhere).Where();
                (query as IQueryFilterGroup).BetweenDates(timeSearchString, startTime.Value, endTime.Value);
            }

            return query;
        }

        private void SearchQueryBuilder(string searchString, IQueryBuilder query, string searchdestination = null)
        {
            Regex regex = new Regex(@"^[a-zA-Z0-9\s-]*$");
            if (!regex.IsMatch(searchString))
                throw new ArgumentException(@"Search string contains unsupported character(s). Supported string is alphanumeric with space or -");
            //As ADT search is case sensitive, adding title case check
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            string titleCaseString = myTI.ToTitleCase(searchString);
            string searchStringLower = searchString.ToLower();
            string searchStringUpper = searchString.ToUpper();
            var dtIdString = "$dtId";
            var nameString = "name";
            if (searchdestination != null)
            {
                dtIdString = string.Concat(searchdestination, ".", dtIdString);
                nameString = string.Concat(searchdestination, ".", nameString);
            }

            (query as IQueryWhere).Where()
                .OpenGroupParenthesis()
                    .Contains(dtIdString, searchString)
                .Or()
                    .Contains(nameString, searchString)
                .Or()
                    .Contains(dtIdString, titleCaseString)
                .Or()
                    .Contains(nameString, titleCaseString)
                .Or()
                    .Contains(dtIdString, searchStringLower)
                .Or()
                    .Contains(nameString, searchStringLower)
                .Or()
                    .Contains(dtIdString, searchStringUpper)
                .Or()
                    .Contains(nameString, searchStringUpper)
                .CloseGroupParenthesis();
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

        private async Task<int> GetCountAsync(AzureDigitalTwinsSettings instanceSettings, string query)
        {
            var queryResult = QueryAsync<CountResult>(instanceSettings, query);
            _logger.LogInformation("GetCountAsync query: {Query}", query);

            var page = await queryResult.AsPages().SingleAsync();

            return page.Values.Single().COUNT;
        }

        public class PageQueryResult
        {
            public int NextPage { get; set; }
            public int Total { get; set; }
            public string continuationtoken { get; set; }

        }
    }
}

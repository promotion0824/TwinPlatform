using Azure.DigitalTwins.Core;
using Azure.Identity;
using Azure.Search.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Helpers;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.CognitiveSearch;
using Willow.Model.Adt;
using Willow.Model.Requests;
using Willow.Model.Response;

namespace Willow.AzureDigitalTwins.Api.Services;

/// <summary>
/// Document Search Mode
/// </summary>
public enum DocumentSearchMode
{
    Keyword,
    Vector,
    Hybrid
}

/// <summary>
/// Interface for Azure Cognitive Search Service
/// </summary>
public interface IAcsService
{
    /// <summary>
    /// Pass-through method to directly query unified index.
    /// </summary>
    /// <param name="rawQuery">Query expression in string format.</param>
    /// <param name="options"> Search Options.</param>
    public Task<List<SearchResult<UnifiedItemDto>>> QueryRawUnifiedIndexAsync(string rawQuery, SearchOptions options);

    /// <summary>
    /// Query Document Index
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <param name="documentSearchMode">Search Mode</param>
    /// <param name="skip">Number of result to skip.</param>
    /// <param name="take">Number of result to fetch.</param>
    public Task<DocumentSearchResponse> QueryDocumentIndexAsync(string query, DocumentSearchMode documentSearchMode, int skip = 0, int take = 100);

    /// <summary>
    /// Filter and get twins from ACS Index
    /// </summary>
    /// <param name="request">Instance of <see cref="GetTwinsInfoRequest"/></param>
    /// <param name="pageSize">Number of response to return.</param>
    /// <param name="continuationToken">Continuation token for the request.</param>
    public Task<Page<TwinWithRelationships>> GetTwinsAsync(
            GetTwinsInfoRequest request,
            int pageSize = 100,
            string continuationToken = null);

    /// <summary>
    /// Get Unified Index Document By Twin DtId
    /// </summary>
    /// <param name="Id">DtId of the Twin.</param>
    /// <returns>Return <see cref="UnifiedItemDto"/> if found;else null;</returns>
    public Task<UnifiedItemDto> GetUnifiedIndexDocumentByTwinId(string Id);

    /// <summary>
    /// Get list of Unified Search Index Document By Twin DtId's
    /// </summary>
    /// <param name="Ids">String Enumerable of Twin DtIds.</param>
    /// <returns>Return a list of <see cref="UnifiedItemDto"/> if found;else empty collection.;</returns>
    public Task<List<UnifiedItemDto>> GetUnifiedIndexDocumentsByTwinIds(IEnumerable<string> Ids);

    /// <summary>
    /// Adds a twin to search index.
    /// </summary>
    /// <param name="basicDigitalTwin">Instance of Basic Digital Twin.</param>
    /// Call <see cref="Flush"/> task at the end to ensure your changes are sent to the Search Index.
    public Task QueueForUpsertTwinAsync(BasicDigitalTwin basicDigitalTwin);

    /// <summary>
    /// Removes a search index twin document from the Search Index if the document matching the Id found; else operation ignored.
    /// </summary>
    /// <param name="Id">DtId of the Digital Twin.</param>
    /// <remarks>
    /// Call <see cref="Flush"/> task at the end to ensure your changes are sent to the Index.
    /// </remarks>
    public Task QueueForDeleteTwinAsync(string Id);

    /// <summary>
    /// Flush the local document queue and sends pending changes to the ACS Search Index.
    /// </summary>
    public Task<DocsFlushResults> Flush();

    /// <summary>
    /// Build twin search index Document from Basic Digital Twin
    /// </summary>
    /// <param name="twin">Instance of BasicDigitalTwin.</param>
    public Task<UnifiedItemDto> BuildSearchIndexDocumentFromTwin(BasicDigitalTwin twin);
}

/// <summary>
/// Service implementation for Azure Cognitive Search
/// </summary>
public class AcsService : IAcsService
{
    private readonly ILogger<AcsService> _logger;
    private readonly ISearchService<UnifiedItemDto> _unifiedSearchService;
    private readonly ISearchService<DocumentChunkDto> _documentSearchService;
    private readonly PendingDocsQueue _pendingDocsQueue;
    private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;
    private readonly IAzureDigitalTwinReader _azureDigitalTwinReader;
    private readonly ICustomColumnService _customColumnService;

    private readonly AISearchSettings _searchSettings;
    private readonly DefaultAzureCredential _defaultAzureCredential;

    private readonly IAdxSetupService _adxSetupService;
    private readonly ITelemetryCollector _telemetryCollector;

    public const string TwinTypeName = "twin";
    public const string FullTextExpression = "*";

    private readonly HealthCheckSearch _healthCheckSearch;

    /// <summary>
    /// Instantiate an instance of <see cref="AcsService"/>
    /// </summary>
    /// <param name="logger">Instance of <see cref="ILogger"/></param>
    /// <param name="unifiedSearchService">Instance of <see cref="ISearchService{T}"/> implementation</param>
    /// <param name="documentSearchService">Instance of <see cref="ISearchService{T}"/> implementation</param>
    /// <param name="pendingDocsQueue">Instance of PendingDocsQueue.</param>
    /// <param name="azureDigitalTwinModelParser">Azure Digital Twin Model Parser instance.</param>
    /// <param name="customColumnService">Instance of Custom Column Service.</param>
    /// <param name="adxSetupService">Instance of ADX Setup Service.</param>
    /// <param name="searchSettings">SearchSettings IOptions.</param>
    /// <param name="defaultAzureCredential">DefaultAzureCredential Instance.</param>
    /// <param name="healthCheckSearch"> Health Check Search Instance.</param>
    /// <param name="azureDigitalTwinReader">Azure Digital Twin Reader Instance.</param>
    /// <param name="telemetryCollector">Telemetry Collector.</param>
    public AcsService(ILogger<AcsService> logger,
        ISearchService<UnifiedItemDto> unifiedSearchService,
        ISearchService<DocumentChunkDto> documentSearchService,
        PendingDocsQueue pendingDocsQueue,
        IAzureDigitalTwinModelParser azureDigitalTwinModelParser,
        ICustomColumnService customColumnService,
        IAdxSetupService adxSetupService,
        IOptions<AISearchSettings> searchSettings,
        DefaultAzureCredential defaultAzureCredential,
        HealthCheckSearch healthCheckSearch,
        IAzureDigitalTwinReader azureDigitalTwinReader,
        ITelemetryCollector telemetryCollector)
    {
        _logger = logger;
        _unifiedSearchService = unifiedSearchService;
        _documentSearchService = documentSearchService;
        _pendingDocsQueue = pendingDocsQueue;
        _azureDigitalTwinModelParser = azureDigitalTwinModelParser;
        _customColumnService = customColumnService;
        _adxSetupService = adxSetupService;
        _searchSettings = searchSettings.Value;
        _defaultAzureCredential = defaultAzureCredential;
        _healthCheckSearch = healthCheckSearch;
        _azureDigitalTwinReader = azureDigitalTwinReader;
        _telemetryCollector = telemetryCollector;
    }

    /// <summary>
    /// Pass-through method to directly query unified index.
    /// </summary>
    /// <param name="rawQuery">Query expression in string format.</param>
    /// <param name="options"> Search Options.</param>
    /// <returns>List of instance of <see cref="SearchResult{T}"/></returns>
    public async Task<List<SearchResult<UnifiedItemDto>>> QueryRawUnifiedIndexAsync(string rawQuery, SearchOptions options)
    {
        try
        {
            var request = await _unifiedSearchService.Search(rawQuery, options, new CancellationToken());
            var response = await request.GetResultsAsync().Select(s => new SearchResult<UnifiedItemDto>(s.Document, s.Score ?? 0.0)).ToListAsync();
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSearch, HealthCheckSearch.Healthy, _logger);
            return response;
        }
        catch
        {
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSearch, HealthCheckSearch.FailingCalls, _logger);
            throw;
        }
    }

    /// <summary>
    /// Query Document Index
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <param name="documentSearchMode">Search Mode</param>
    /// <param name="skip">Number of result to skip.</param>
    /// <param name="take">Number of result to fetch.</param>
    /// <returns>DocumentSearchResponse</returns>
    public async Task<DocumentSearchResponse> QueryDocumentIndexAsync(string query, DocumentSearchMode documentSearchMode, int skip = 0, int take = 100)
    {
        try
        {
            var searchOption = new SearchOptions()
            {
                Skip = skip,
                Size = take,
                Select = { nameof(DocumentChunkDto.GroupId), nameof(DocumentChunkDto.Title), nameof(DocumentChunkDto.Content), nameof(DocumentChunkDto.DocLastUpdateTime), nameof(DocumentChunkDto.Uri) },
                IncludeTotalCount = true,
            };
            if (documentSearchMode == DocumentSearchMode.Vector || documentSearchMode == DocumentSearchMode.Hybrid)
            {
                searchOption.VectorSearch = new()
                {
                    Queries = {
                        new Azure.Search.Documents.Models.VectorizableTextQuery(text: query)
                        {
                            KNearestNeighborsCount = skip+take,
                            Fields = { nameof(DocumentChunkDto.ContentVector) },
                            Exhaustive = true
                        }
                    },
                };
            }
            string searchText = documentSearchMode == DocumentSearchMode.Vector ? null : query;

            var searchResult = await _documentSearchService.Search(searchText, searchOption, default);

            DocumentSearchResponse response = new(skip, take, searchResult.TotalCount ?? (skip + take));

            await foreach (var item in searchResult.GetResultsAsync())
            {
                response.AddResult(new DocumentSearchResult()
                {
                    Id = item.Document.GroupId,
                    Title = item.Document.Title,
                    Path = item.Document.Uri,
                    LastModified = item.Document.DocLastUpdateTime,
                    Chunks = [new ScoredDocumentChunk(item.Document.Content, item.Score ?? 0)]
                });
            }

            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSearch, HealthCheckSearch.Healthy, _logger);
            return response;
        }
        catch (Exception)
        {
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSearch, HealthCheckSearch.FailingCalls, _logger);
            throw;
        }
    }


    public async Task<Page<TwinWithRelationships>> GetTwinsAsync(
        GetTwinsInfoRequest request,
        int pageSize = 100,
        string continuationToken = null)
    {
        try
        {
            var twinsWithRelationship = await this.GetTwinsHelperAsync(request, pageSize, continuationToken);
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSearch, HealthCheckSearch.Healthy, _logger);
            return twinsWithRelationship;
        }
        catch
        {
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckSearch, HealthCheckSearch.FailingCalls, _logger);
            throw;
        }
    }

    /// <summary>
    /// Filter and get twins from ACS Index
    /// </summary>
    /// <param name="request">Instance of <see cref="GetTwinsInfoRequest"/></param>
    /// <param name="pageSize">Number of response to return.</param>
    /// <param name="continuationToken">Continuation token for the request.</param>
    /// <returns>Task for Page of <see cref="TwinWithRelationships"/></returns>
    private async Task<Page<TwinWithRelationships>> GetTwinsHelperAsync(
            GetTwinsInfoRequest request,
            int pageSize = 100,
            string continuationToken = null)
    {
        var searchFilter = QueryExpressionBuilder.Create() as IQueryFilter;

        // Add the index Filter
        searchFilter.FilterEqual(nameof(UnifiedItemDto.Type), TwinTypeName);

        // Add Model Filter
        if (request.ModelId is not null && request.ModelId.Any())
        {
            searchFilter.And();
            searchFilter.OpenParenthesis();

            // If Exact Model match search in Primary Model Id; else in ModelIds
            _ = request.ExactModelMatch ? searchFilter.SearchIn(nameof(UnifiedItemDto.PrimaryModelId), request.ModelId) :
                 searchFilter.SearchInArray(nameof(UnifiedItemDto.ModelIds), request.ModelId);

            searchFilter.CloseParenthesis();
        }

        // Add Location Filter
        if (!string.IsNullOrWhiteSpace(request.LocationId))
        {
            searchFilter.And();
            searchFilter.OpenParenthesis();
            // filter by Site Id field
            searchFilter.FilterEqual(nameof(UnifiedItemDto.SiteId), request.LocationId);
            searchFilter.Or();
            // Look for values in the Location array
            searchFilter.FilterContains(nameof(UnifiedItemDto.Location), request.LocationId);
            searchFilter.CloseParenthesis();
        }

        var searchOption = new SearchOptions()
        {
            Filter = searchFilter.GetQuery(),
            Skip = Convert.ToInt32(continuationToken),
            Size = pageSize
        };

        if (string.IsNullOrWhiteSpace(request.SearchString))
        {
            request.SearchString = FullTextExpression;
        }

        var twinDocuments = await _unifiedSearchService.Search(request.SearchString, searchOption, new CancellationToken());
        var twins = twinDocuments.GetResults().Select(s => MapSearchDocToTwin(s.Document)).ToList();
        return new Page<TwinWithRelationships> { Content = twins, ContinuationToken = (searchOption.Skip + searchOption.Size).ToString() };
    }

    /// <summary>
    /// Get Unified Index Document By Twin DtId
    /// </summary>
    /// <param name="Id">DtId of the Twin.</param>
    /// <returns>Return <see cref="UnifiedItemDto"/> if found;else null;</returns>
    public async Task<UnifiedItemDto> GetUnifiedIndexDocumentByTwinId(string Id)
    {
        var searchFilter = QueryExpressionBuilder.Create() as IQueryFilter;
        // Add the index Filter
        searchFilter.FilterEqual(nameof(UnifiedItemDto.Type), TwinTypeName);
        searchFilter.And();
        searchFilter.SearchInArray(nameof(UnifiedItemDto.Ids), new[] { Id });

        var searchOption = new SearchOptions()
        {
            Filter = searchFilter.GetQuery(),
            Skip = 0,
            Size = 1
        };

        var twinDocuments = await _unifiedSearchService.Search(FullTextExpression, searchOption, new CancellationToken());
        return twinDocuments.GetResults().SingleOrDefault()?.Document;
    }

    /// <summary>
    /// Get list of Unified Search Index Document By Twin DtId's
    /// </summary>
    /// <param name="Ids">String Enumerable of Twin DtIds.</param>
    /// <returns>Return a list of <see cref="DocumentChunkDto"/> if found;else empty collection.;</returns>
    public async Task<List<UnifiedItemDto>> GetUnifiedIndexDocumentsByTwinIds(IEnumerable<string> Ids)
    {
        var searchFilter = QueryExpressionBuilder.Create() as IQueryFilter;
        // Add the index Filter
        searchFilter.FilterEqual(nameof(UnifiedItemDto.Type), TwinTypeName);
        searchFilter.And();
        searchFilter.SearchInArray(nameof(UnifiedItemDto.Ids), Ids);

        var searchOption = new SearchOptions()
        {
            Filter = searchFilter.GetQuery(),
            Skip = 0,
            Size = Ids.Count()
        };

        var twinDocuments = await _unifiedSearchService.Search(FullTextExpression, searchOption, new CancellationToken());
        var result = twinDocuments.GetResults().Select(s => s.Document).ToList();

        var duplicatedIds = result.GroupBy(g => g.Id).Where(a => a.Count() > 1).Select(s => s.Key).ToList();
        if (duplicatedIds.Any())
        {
            _logger.LogWarning("Found twin document with duplicated Id(s): {Ids}", string.Join(',', duplicatedIds));
        }

        return result;
    }

    /// <summary>
    /// Adds a twin Document to search index.
    /// </summary>
    /// <param name="basicDigitalTwin">Instance of Basic Digital Twin.</param>
    /// Call <see cref="Flush"/> task at the end to ensure your changes are sent to the Search Index.
    /// <returns>Task that can be awaited.</returns>
    public async Task QueueForUpsertTwinAsync(BasicDigitalTwin basicDigitalTwin)
    {
        var searchDocToUpload = await BuildSearchIndexDocumentFromTwin(basicDigitalTwin);
        var searchClient = GetSearchClient(_searchSettings.UnifiedIndexName);
        await _pendingDocsQueue.Upload(searchClient, searchDocToUpload, new CancellationToken());
        _telemetryCollector.TrackACSIndexAdditionsCount(1);
    }

    /// <summary>
    /// Removes a search index twin document from the Search Index if the document matching the Id found; else operation ignored.
    /// </summary>
    /// <param name="Id">DtId of the Digital Twin.</param>
    /// <remarks>
    /// Call <see cref="Flush"/> task at the end to ensure your changes are sent to the Index.
    /// </remarks>
    /// <returns>Task that can be awaited.</returns>
    public async Task QueueForDeleteTwinAsync(string Id)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(Id);
        var searchDocToDelete = await GetUnifiedIndexDocumentByTwinId(Id);
        if (searchDocToDelete is null)
            return;
        var searchClient = GetSearchClient(_searchSettings.UnifiedIndexName);
        await _pendingDocsQueue.Delete(searchClient, searchDocToDelete, new CancellationToken());
        _telemetryCollector.TrackACSIndexDeletionsCount(1);
    }

    /// <summary>
    /// Flush the local twin document queue and send changes to the ACS Index.
    /// </summary>
    /// <returns>Task that can be awaited.</returns>
    public async Task<DocsFlushResults> Flush() => await _pendingDocsQueue.Flush(GetSearchClient(_searchSettings.UnifiedIndexName), new CancellationToken());

    private static TwinWithRelationships MapSearchDocToTwin(UnifiedItemDto dto)
    {
        return new TwinWithRelationships()
        {
            Twin = new BasicDigitalTwin()
            {
                Id = dto.Id,
                Metadata = new DigitalTwinMetadata()
                {
                    ModelId = dto.PrimaryModelId
                },
                Contents = new Dictionary<string, object>()
                {
                    // ACS Index store multiple names, so just get the first one
                    { "name", dto.Names.FirstOrDefault() },
                    { "siteID", dto.SiteId },
                    { "externalID", dto.ExternalId }
                }
            }
        };
    }

    /// <summary>
    /// Build Search Index Document from Basic Digital Twin
    /// </summary>
    /// <param name="twin">Instance of BasicDigitalTwin.</param>
    /// <returns>Instance of Search Index DTO.</returns>
    public async Task<UnifiedItemDto> BuildSearchIndexDocumentFromTwin(BasicDigitalTwin twin)
    {
        try
        {
            int twinDefaultScore = 80; // Got this from Rules Engine
            string name = GetStringValueOrNull(twin.Contents, "name");
            string description = GetStringValueOrNull(twin.Contents, "description");

            string uniqueId = GetStringValueOrNull(twin.Contents, "uniqueID");
            string trendId = GetStringValueOrNull(twin.Contents, "trendID");
            string externalId = GetStringValueOrNull(twin.Contents, "externalID");
            string connectorId = GetStringValueOrNull(twin.Contents, "connectorID");

            var modelInterfaceInfo = _azureDigitalTwinModelParser.GetInterfaceInfo(twin.Metadata.ModelId);
            var modelName = GetLocaleStringOrDefault(modelInterfaceInfo.DisplayName.ToDictionary(p => p.Key, p => p.Value), defaultLanguageCode);
            var allModelAncestorIds = _azureDigitalTwinModelParser.GetAllAncestors(twin.Metadata.ModelId);
            var allModelAncestorNames = allModelAncestorIds.Select(s =>
                                        GetLocaleStringOrDefault(_azureDigitalTwinModelParser.GetInterfaceInfo(s).DisplayName.ToDictionary(p => p.Key, p => p.Value),
                                                                 defaultLanguageCode));

            var customCols = await _adxSetupService.GetAdxTableSchema();
            var locationCustomCol = customCols.Single(x => x.Name == "Location");
            var twinSerialized = JsonDocument.Parse(JsonSerializer.Serialize(twin)).RootElement;

            // Note: Custom Column Service cache any ADT Match query related calls for 10 seconds
            // Assuming the following location calculation complete within 10 seconds,
            // there will be a max of one to call to get the site twin and another to get the floor twin
            string siteId = await _customColumnService.CalculateColumn(customCols.Single(s => s.Name == "SiteId"), twin, twinSerialized, null, false, reEvaluate: false);
            var locationIds = await Task.WhenAll(locationCustomCol.Children.Where(w => new[] { "SiteId", "SiteDtId", "FloorId", "FloorDtId" }.Contains(w.Name))
                                                .Select(s => _customColumnService.CalculateColumn(s, twin, twinSerialized, null, false, true)));
            var locationNames = await Task.WhenAll(locationCustomCol.Children.Where(w => new[] { "SiteName", "FloorName" }.Contains(w.Name))
                                        .Select(s => _customColumnService.CalculateColumn(s, twin, twinSerialized, null, false, true)));

            // FedBy Ancestor Ids
            var fedByAncestors = await _azureDigitalTwinReader.GetTwinRelationshipsAsync(twin.Id, "isFedBy");
            // Feeds Ancestor Ids
            var feedsAncestors = (await _azureDigitalTwinReader.GetIncomingRelationshipsAsync(twin.Id)).Where(w => w.Name == "isFedBy");

            Tags tags = null;
            if (twin.Contents.TryGetValue("tags", out object tagsValue))
            {
                try
                {
                    var tagDict = JsonSerializer.Deserialize<Dictionary<string, bool>>(tagsValue.ToString());
                    tags = new Tags(tagDict?.Keys ?? Enumerable.Empty<string>());
                }
                catch (Exception)
                {
                    _logger.LogError("ACS Service: Error parsing tag column for twin:{twinId}", twin.Id);
                }

            }
            else
            {
                tags = new Tags();
            }


            var searchDoc = new UnifiedItemDto(TwinTypeName,
                id: twin.Id,
                names: new Names(name),
                secondaryNames: new Names(description, name?.Replace('_', ' ')),
                ids: new Ids(twin.Id, uniqueId, trendId, externalId, connectorId),
                siteId: siteId,
                externalId: externalId,
                locationAncestors: new LocationAncestorIds(locationIds),
                locationNames: new LocationNames(locationNames),
                fedByAncestors: new FedByAncestorIds(fedByAncestors.Select(s => s.TargetId).ToArray()),
                feedsAncestors: new FeedsAncestorIds(feedsAncestors.Select(s => s.SourceId).ToArray()),
                tenantAncestors: new TenantAncestorIds(),
                primaryModelId: twin.Metadata.ModelId,
                modelids: new ModelIds(allModelAncestorIds.Union(new[] { twin.Metadata.ModelId }).ToArray()),
                modelNames: new ModelNames(allModelAncestorNames.Union(new[] { modelName }).ToArray()),
                tags: tags,
                category: modelName,
                importance: twinDefaultScore
                )
            {
                // twin last Update time; As we saw in few cases ADT LastUpdateOn may ne null, in that case use current UTC time.
                Latest = twin.LastUpdatedOn ?? DateTimeOffset.Now,
                IndexedDate = DateTimeOffset.UtcNow
            };

            return searchDoc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building index from document twin");
            throw;
        }
    }

    const string defaultLanguageCode = "en";
    private static string GetStringValueOrNull(IDictionary<string, object> twinContent, string keyName)
    {
        if (twinContent.TryGetValue(keyName, out object value))
        {

            if (value is JsonElement jsonValue)
            {
                return jsonValue.ToString();
            }

            return value as string ?? GetLocaleStringOrDefault(value, defaultLanguageCode);
        }
        return null;
    }

    private static string GetLocaleStringOrDefault(object objValue, string languageCode)
    {
        var dictValues = objValue as Dictionary<string, string>;

        // Note that the non-nullable struct KeyValuePair<string,string> has the default (null,null)
        return dictValues?.FirstOrDefault(f => f.Key == languageCode).Value ?? dictValues?.Values?.FirstOrDefault();
    }

    private SearchClient GetSearchClient(string indexName)
    {
        Uri endpoint = new(_searchSettings.Uri);
        _logger.LogTrace("Search service configured {endpoint}", endpoint);
        return new SearchClient(endpoint, indexName, _defaultAzureCredential);
    }
}

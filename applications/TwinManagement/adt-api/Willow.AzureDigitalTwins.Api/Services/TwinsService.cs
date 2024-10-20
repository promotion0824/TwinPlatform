using Abodit.Mutable;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Diagnostic;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Helpers;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Copilot.ProxyAPI;
using Willow.Model.Adt;
using Willow.Model.Requests;
using Willow.Model.Responses;

namespace Willow.AzureDigitalTwins.Api.Services
{
    public interface ITwinsService
    {
        Task<Page<TwinWithRelationships>> AppendRelationships(Page<BasicDigitalTwin> page, bool includeRelationships, bool includeIncomingRelationships, bool twinIdsOnly = false);
        Task<Graph<EquatableBasicDigitalTwin, TwinRelation>> GetTwinSystemGraph(string[] twinIds);
        Task<BasicDigitalTwin> GetDocumentTwin(string twinId);
        Task<Page<TwinWithRelationships>> GetTwins(
            GetTwinsInfoRequest request,
            int pageSize = 100,
            string continuationToken = null,
            bool includeTotalCount = false);
        bool IsValidDocumentTwin(BasicDigitalTwin basicDigitalTwin);

        /// <summary>
        /// Get twins by Ids and its children in tree form.
        /// </summary>
        /// <param name="twinScopeIds">Twin Ids for the tree root. Only the descendants of the twins will be returned.</param>
        /// <param name="childModels">Target model ids of the twin to be on the response.</param>
        /// <param name="outgoingRelationships">List of relationship types to be considered for traversal.</param>
        /// <param name="incomingRelationships">List of relationship types to be considered for traversal.</param>
        Task<IEnumerable<NestedTwin>> GetTreeByIdsAsync(IEnumerable<string> twinScopeIds, IEnumerable<string> childModels, IEnumerable<string> outgoingRelationships, IEnumerable<string> incomingRelationships);

        /// <summary>
        /// Get twins and its children in tree form.
        /// </summary>
        /// <param name="rootModels">Model Ids of the root twins.</param>
        /// <param name="childModels">Target model ids of the twin to be on the response.</param>
        /// <param name="outgoingRelationships">List of relationship types to be considered for traversal.</param>
        /// <param name="incomingRelationships">List of relationship types to be considered for traversal.</param>
        /// <param name="exactModelMatch">Indicates if model filter must be exact match</param>
        Task<IEnumerable<NestedTwin>> GetTreeByModelsAsync(IEnumerable<string> rootModels, IEnumerable<string> childModels, IEnumerable<string> outgoingRelationships, IEnumerable<string> incomingRelationships, bool exactModelMatch = false);

        Task<int> GetTwinsCount(GetTwinsInfoRequest request);

        Task<MultipleEntityResponse> DeleteTwinsAndRelationships(
            IEnumerable<string> twinIds,
            bool deleteRelationships = false);
        Task<BasicDigitalTwin> CreateOrReplaceDigitalTwinAsync(BasicDigitalTwin twin, CancellationToken cancellationToken = default);
        Task<Page<TwinWithRelationships>> GetTwinsByIds(
                                                string[] twinId,
                                                SourceType sourceType,
                                                bool includeRelationships);

        Task<Dictionary<string, int>> GetTwinCountByModelAsync(IEnumerable<string> modelIds, string locationId, SourceType sourceType, bool forceRefreshCache = false);
        Task<Page<TwinWithRelationships>> QueryTwinsAsync(
            QueryTwinsRequest request,
            SourceType sourceType,
            int pageSize = 100,
            string continuationToken = null);

        Task<Page<JsonDocument>> QueryAsync(
            string query,
            SourceType sourceType,
            int pageSize = 100,
            string continuationToken = null);

        Task<bool> TryDeleteDocFromCopilot(string blobName);
    }
    public class TwinsService : ITwinsService
    {
        private readonly IAzureDigitalTwinReader _azureDigitalTwinReader;
        private readonly ILogger<TwinsService> _logger;
        private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;
        private readonly IAdxService _adxService;
        private readonly IAzureDigitalTwinWriter _azureDigitalTwinWriter;
        private readonly IExportService _exportService;
        private readonly ICustomColumnService _customColumnService;
        private readonly IAcsService _acsService;
        private const string _lastUpdateTimeColumnAlias = "LastUpdateTime";
        private readonly IMemoryCache _memoryCache;
        private readonly DateTimeOffset CacheTimeoutModelCount = DateTimeOffset.UtcNow.AddMinutes(10);
        private readonly HealthCheckADT _healthCheckAdt;
        private readonly ITelemetryCollector _telemetryCollector;
        private readonly ICopilotClient? _copilotClient;

        public TwinsService(
            IAzureDigitalTwinReader azureDigitalTwinReader,
            ILogger<TwinsService> logger,
            IAzureDigitalTwinModelParser azureDigitalTwinModelParser,
            IAzureDigitalTwinWriter azureDigitalTwinWriter,
            IExportService exportService,
            IAdxService adxService,
            ICustomColumnService customColumnService,
            IAcsService acsService,
            IMemoryCache memoryCache,
            HealthCheckADT healthCheckADT,
            ITelemetryCollector telemetryCollector,
            IOptionalDependency<ICopilotClient> copilotClient)
        {
            _azureDigitalTwinReader = azureDigitalTwinReader;
            _logger = logger;
            _azureDigitalTwinModelParser = azureDigitalTwinModelParser;
            _azureDigitalTwinWriter = azureDigitalTwinWriter;
            _adxService = adxService;
            _exportService = exportService;
            _customColumnService = customColumnService;
            _acsService = acsService;
            _memoryCache = memoryCache;
            _healthCheckAdt = healthCheckADT;
            _telemetryCollector = telemetryCollector;
            _copilotClient = copilotClient.Value;
        }

        /// <summary>
        /// CreateOrUpdate the specified twin.
        /// If there is no UniqueId property set or the twin does not exist, one will be created, otherwise attempt to lookup the existing one and re-use it.
        /// Analogous treatment is given to the TrendId property, but only if the twin is a Capability.
        /// TODO: Remove this logic once our services no longer require a uniqueId.
        /// </summary>
        public async Task<BasicDigitalTwin> CreateOrReplaceDigitalTwinAsync(BasicDigitalTwin twin, CancellationToken cancellationToken = default)
        {
            await PresetUniqueIdAndTrendId(twin);
            return await _azureDigitalTwinWriter.CreateOrReplaceDigitalTwinAsync(twin, cancellationToken);
        }

        public async Task<Page<TwinWithRelationships>> AppendRelationships(Page<BasicDigitalTwin> page, bool includeRelationships, bool includeIncomingRelationships, bool twinIdsOnly = false)
        {
            var twinsWithRelationships = await _azureDigitalTwinReader.AppendRelationships(page.Content, includeRelationships, includeIncomingRelationships);

            var apiTwinsWithRelationships = twinsWithRelationships.Select(x =>
            {
                var twin = x.Item1;
                if (twinIdsOnly)
                {
                    twin.ETag = null;
                    twin.Contents = new Dictionary<string,object>();
                    twin.Metadata.PropertyMetadata = new Dictionary<string, DigitalTwinPropertyMetadata>();
                }

                return new TwinWithRelationships { Twin = twin, OutgoingRelationships = x.Item2.Any() ? x.Item2.ToList() : null, IncomingRelationships = x.Item3.Any() ? x.Item3.ToList() : null };
            }).ToList();

            return new Page<TwinWithRelationships> { Content = apiTwinsWithRelationships, ContinuationToken = page.ContinuationToken };
        }

        public async Task<BasicDigitalTwin> GetDocumentTwin(string twinId)
        {
            var twin = await _azureDigitalTwinReader.GetDigitalTwinAsync(twinId);
            if (twin == null)
                return null;

            if (!_azureDigitalTwinModelParser.IsDescendantOf(ModelDefinitions.Documents, twin.Metadata.ModelId))
                return null;

            return twin;
        }

        public bool IsValidDocumentTwin(BasicDigitalTwin basicDigitalTwin)
        {
            var documentModels = _azureDigitalTwinModelParser.GetInterfaceDescendants(new List<string> { ModelDefinitions.Documents });

            return documentModels.Any(x => x.Key == basicDigitalTwin.Metadata.ModelId);
        }

        /// <summary>
        /// Get twins by Ids and its children in tree form.
        /// </summary>
        /// <param name="twinScopeIds">Twin Ids for the tree root. Only the descendants of the twins will be returned.</param>
        /// <param name="childModels">Target model ids of the twin to be on the response.</param>
        /// <param name="outgoingRelationships">List of relationship types to be considered for traversal.</param>
        /// <param name="incomingRelationships">List of relationship types to be considered for traversal.</param>
        public async Task<IEnumerable<NestedTwin>> GetTreeByIdsAsync(
            IEnumerable<string> twinScopeIds,
            IEnumerable<string> childModels,
            IEnumerable<string> outgoingRelationships,
            IEnumerable<string> incomingRelationships)
        {
            IEnumerable<BasicDigitalTwin> twins = (await _azureDigitalTwinReader.GetTwinsByIdsAsync(twinScopeIds)).Content.ToList();
            if (twins == null || !twins.Any())
                return Enumerable.Empty<NestedTwin>();

            var nestedTwins = twins.Select(x => new NestedTwin(x)).ToList();
            if (!outgoingRelationships.Any() && !incomingRelationships.Any())
                return nestedTwins;

            var twinsMap = await GetTreeAsync(nestedTwins, childModels, outgoingRelationships, incomingRelationships);
            return twinsMap.Where(w => twinScopeIds.Any(a => a == w.Value.ParentId)).Select(s => s.Value);
        }

        /// <summary>
        /// Get twins and its children in tree form.
        /// </summary>
        /// <param name="rootModels">Model Ids of the root twins.</param>
        /// <param name="childModels">Target model ids of the twin to be on the response.</param>
        /// <param name="outgoingRelationships">List of relationship types to be considered for traversal.</param>
        /// <param name="incomingRelationships">List of relationship types to be considered for traversal.</param>
        /// <param name="exactModelMatch">Indicates if model filter must be exact match</param>
        public async Task<IEnumerable<NestedTwin>> GetTreeByModelsAsync(
            IEnumerable<string> rootModels,
            IEnumerable<string> childModels,
            IEnumerable<string> outgoingRelationships,
            IEnumerable<string> incomingRelationships,
            bool exactModelMatch = false)
        {
            var request = new GetTwinsInfoRequest
            {
                ModelId = rootModels.ToArray(),
                ExactModelMatch = exactModelMatch
            };
            var page = await _azureDigitalTwinReader.GetTwinsAsync(request);
            IEnumerable<BasicDigitalTwin> twins = await page.FetchAll(x => _azureDigitalTwinReader.GetTwinsAsync(request, continuationToken: x.ContinuationToken));
            if (twins == null || !twins.Any())
                return Enumerable.Empty<NestedTwin>();

            var nestedTwins = twins.Select(x => new NestedTwin(x)).ToList();
            if (!outgoingRelationships.Any() && !incomingRelationships.Any())
                return nestedTwins;
            var twinsMap = await GetTreeAsync(nestedTwins, childModels, outgoingRelationships, incomingRelationships);
            return twinsMap.Where(w => string.IsNullOrWhiteSpace(w.Value.ParentId)).Select(s => s.Value);
        }


        private async Task<ConcurrentDictionary<string, NestedTwin>> GetTreeAsync(
            IEnumerable<NestedTwin> rootNestedTwins,
            IEnumerable<string> childModels,
            IEnumerable<string> outgoingRelationships,
            IEnumerable<string> incomingRelationships)
        {
            // Use Extended Cache if the reader supports the implementation. Only AzureDigitalTwinCacheReader currently supports.
            (_azureDigitalTwinReader as IAzureDigitalTwinReaderCacheSelector)?.SetCacheType(useExtended: true);

            var processQueue = new List<NestedTwin>(rootNestedTwins);
            var seen = new ConcurrentBag<string>();

            // Dictionary's key is Nested twin's id, value is a tuple where item1 is parent's id and item2 is NestedTwin.
            var twinsMap = new ConcurrentDictionary<string,  NestedTwin>();

            while (processQueue.Count > 0)
            {
                var newTwinsToProcess = new ConcurrentBag<NestedTwin>();
                await Parallel.ForEachAsync(processQueue, async (currentTwin, token) =>
                {
                    seen.Add(currentTwin.Twin.Id);

                    var parentIds = new ConcurrentBag<string>();

                    await FollowRelationshipsAsync(outgoingRelationships,
                        processQueue,
                        seen,
                        newTwinsToProcess,
                        x => x.TargetId,
                        () => _azureDigitalTwinReader.GetTwinRelationshipsAsync(currentTwin.Twin.Id, outgoingRelationships.Count() == 1 ? outgoingRelationships.First() : null),
                        parentId => parentIds.Add(parentId));

                    await FollowRelationshipsAsync(incomingRelationships,
                        processQueue,
                        seen,
                        newTwinsToProcess,
                        x => x.SourceId,
                        () => _azureDigitalTwinReader.GetIncomingRelationshipsAsync(currentTwin.Twin.Id));

                    currentTwin.ParentId = parentIds.Distinct().FirstOrDefault();
                    twinsMap.TryAdd(currentTwin.Twin.Id, currentTwin);
                });

                processQueue.Clear();
                foreach (var twinToProcess in newTwinsToProcess)
                {
                    // Traverse the tree only if twins falls under the child models; if empty ignore the check and consider twins of any models
                    if (childModels == null || !childModels.Any() || childModels.Any(m => m == twinToProcess.Twin.Metadata.ModelId))
                    {
                        processQueue.Add(twinToProcess);
                    }
                }
                newTwinsToProcess.Clear();
            }

            foreach (var twinInfo in twinsMap.Select(x => x.Value))
            {
                if (!string.IsNullOrEmpty(twinInfo.ParentId) && twinsMap.TryGetValue(twinInfo.ParentId, out NestedTwin target))
                    target.Children.Add(twinInfo);
            }

            return twinsMap;
        }

        public async Task<int> GetTwinsCount(GetTwinsInfoRequest request)
        {
            var result = 0;
            switch (request.SourceType)
            {
                case SourceType.Adx:
                    {
                        result = await _adxService.GetTwinsCount(request);
                        break;
                    }
                case SourceType.AdtQuery:
                    {
                        result = await _azureDigitalTwinReader.GetTwinsCountAsyncWithSearch(request);
                        break;
                    }
            }
            return result;
        }

        /// <summary>
        /// Delete each twin in the list and optionally delete all incoming and/or outgoing relationships.
        /// Currently will throw an exception for the first twin or relationship fails to delete.
        /// TODO: We could do as much work as possible and collect errors in a custom response --
        ///   this way is a bit safer, though perhaps less convenient in some cases
        /// </summary>
        public async Task<MultipleEntityResponse> DeleteTwinsAndRelationships(
            IEnumerable<string> twinIds,
            bool deleteRelationships = false)
        {
            var resp = new MultipleEntityResponse();

            await Parallel.ForEachAsync(twinIds, async (id, _) =>
            {
                resp.Merge(await DeleteTwinAndRelationships(id, deleteRelationships));
            });

            //Count Ok responses
            _telemetryCollector.TrackTwinWithRelationshipsDeleteSuccess(resp.Responses.LongCount(x => x.StatusCode == HttpStatusCode.OK));

            //Count non-Ok responses
            _telemetryCollector.TrackTwinWithRelationshipsDeleteFailure(resp.Responses.LongCount(x => x.StatusCode != HttpStatusCode.OK));

            return resp;
        }

        public async Task<Page<TwinWithRelationships>> GetTwins(
            GetTwinsInfoRequest request,
            int pageSize = 100,
            string continuationToken = null,
            bool includeTotalCount = false)
        {
            try
            {
                var twinWithRelationship = await this.GetTwinsHelper(request, pageSize, continuationToken, includeTotalCount);

                if (request.SourceType == SourceType.AdtQuery)
                    HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckAdt, HealthCheckADT.Healthy, _logger);

                return twinWithRelationship;
            }
            catch
            {
                if (request.SourceType == SourceType.AdtQuery)
                    HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckAdt, HealthCheckADT.FailingCalls, _logger);

                throw;
            }
        }

        private async Task<Page<TwinWithRelationships>> GetTwinsHelper(
            GetTwinsInfoRequest request,
            int pageSize = 100,
            string continuationToken = null,
            bool includeTotalCount = false)
        {
            Page<TwinWithRelationships> twinsWithRelationships = null;

            switch (request.SourceType)
            {
                case SourceType.Acs:
                    {
                        using (_telemetryCollector.StartActivity("GetTwinsSearchDtosAcs", ActivityKind.Consumer))
                            twinsWithRelationships = await _acsService.GetTwinsAsync(
                                                                        request,
                                                                        pageSize,
                                                                        continuationToken);
                        break;
                    }
                case SourceType.Adx:
                    {
                        using (_telemetryCollector.StartActivity("GetTwinsAdx", ActivityKind.Consumer))
                            twinsWithRelationships = await _adxService.GetTwins(
                                                                    request,
                                                                    pageSize,
                                                                    continuationToken);
                        break;
                    }
                case SourceType.AdtMemory:
                    {
                        Page<BasicDigitalTwin> twins;
                        if (!string.IsNullOrEmpty(request.LocationId))
                        {
                            var pageNumber = int.TryParse(continuationToken, out int localNumber) ? localNumber : 1;
                            var inMemoryTwins = await FollowAllRelsToTargetModel(request.LocationId, request.RelationshipsToTraverse, request.ModelId, request.ExactModelMatch, (pageSize * pageNumber) + 1);
                            twins = inMemoryTwins.ToPageModel(pageNumber, pageSize);
                        }
                        else
                        {
                            twins = await _azureDigitalTwinReader.GetTwinsAsync(
                                                       request,
                                                       pageSize: pageSize,
                                                       continuationToken: continuationToken);
                        }

                        twinsWithRelationships = await AppendRelationships(twins, request.IncludeRelationships, request.IncludeIncomingRelationships);

                        break;
                    }
                case SourceType.AdtQuery:
                    {
                        using (_telemetryCollector.StartActivity("GetTwinsAdt", ActivityKind.Consumer))
                        {
                            var twins = await _azureDigitalTwinReader.GetTwinsAsync(request,
                                                       null,
                                                       pageSize,
                                                       includeCountQuery: includeTotalCount,
                                                       continuationToken);

                            twinsWithRelationships = await AppendRelationships(twins, request.IncludeRelationships, request.IncludeIncomingRelationships);
                        }

                        break;
                    }
            }

            // Copy native BasicDigitalTwin LastUpdatedOn (aka. $lastUpdateTime prop) to a new lastUpdateTime prop
            // since LastUpdatedOn  didn't get properly serialized in the frontEnd
            foreach (var twinWithRel in twinsWithRelationships.Content)
            {
                if (!twinWithRel.Twin.LastUpdatedOn.HasValue && twinWithRel.TwinData == null && !(twinWithRel.TwinData?.ContainsKey(_lastUpdateTimeColumnAlias) is true))
                {
                    _logger.LogWarning("Missing LastUpdatedOn for twin {twinId}", twinWithRel?.Twin?.Id ?? "?");
                    continue;
                }
                var twinData = twinWithRel.TwinData ?? new Dictionary<string, object>();
                if (!twinData.ContainsKey(_lastUpdateTimeColumnAlias))
                    twinData.Add(_lastUpdateTimeColumnAlias, ((DateTimeOffset)twinWithRel.Twin.LastUpdatedOn).UtcDateTime);
                twinWithRel.TwinData = twinData;
            }

            return twinsWithRelationships;
        }

        public async Task<Page<TwinWithRelationships>> GetTwinsByIds(string[] twinId,
                                                             SourceType sourceType,
                                                             bool includeRelationships)
        {
            Page<TwinWithRelationships> twinWithRelationships = null;

            switch (sourceType)
            {
                case SourceType.Adx:
                    {
                        twinWithRelationships = await _adxService.GetTwinsByIds(twinId, includeRelationships);
                        break;
                    }

                case SourceType.AdtQuery:
                    {
                        try
                        {
                            var twins = await _azureDigitalTwinReader.GetTwinsByIdsAsync(twinId);

                            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckAdt, HealthCheckADT.Healthy, _logger);

                            twinWithRelationships = await AppendRelationships(twins, includeRelationships, includeRelationships);

                            break;
                        }
                        catch
                        {
                            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckAdt, HealthCheckADT.FailingCalls, _logger);
                            throw;
                        }
                    }
            }
            return twinWithRelationships;
        }

        public async Task<Page<TwinWithRelationships>> QueryTwinsAsync(
            QueryTwinsRequest request,
            SourceType sourceType,
            int pageSize = 100,
            string continuationToken = null)
        {
            Page<TwinWithRelationships> twinsWithRelationships = null;
            if (sourceType == SourceType.Adx)
            {
                twinsWithRelationships = await _adxService.QueryTwinsAsync(request.Query, request.IncludeRelationships, request.IncludeIncomingRelationships, pageSize, HeaderUtilities.UnescapeAsQuotedString(continuationToken).Value);
            }
            else
            {
                var digitalTwins = await _azureDigitalTwinReader.QueryTwinsAsync(request.Query, pageSize, HeaderUtilities.UnescapeAsQuotedString(continuationToken).Value);

                twinsWithRelationships = await AppendRelationships(digitalTwins, request.IncludeRelationships, request.IncludeIncomingRelationships, request.IdsOnly);
            }

            return twinsWithRelationships;
        }

        public async Task<Page<JsonDocument>> QueryAsync(
            string query,
            SourceType sourceType,
            int pageSize = 100,
            string continuationToken = null)
        {
            if (sourceType == SourceType.Adx)
            {
                return await _adxService.QueryAsync(query, pageSize: pageSize, HeaderUtilities.UnescapeAsQuotedString(continuationToken).Value);
            }
            var results = _azureDigitalTwinReader.QueryAsync<JsonDocument>(query);

            var page = await results.AsPages(continuationToken).FirstOrDefaultAsync();

            return new Page<JsonDocument> { Content = page?.Values, ContinuationToken = page?.ContinuationToken };
        }

        public async Task<Graph<EquatableBasicDigitalTwin, TwinRelation>> GetTwinSystemGraph(string[] twinIds)
        {
            Graph<EquatableBasicDigitalTwin, TwinRelation> result = new();

            // Identity map all the basic digital twin poco
            ConcurrentDictionary<string, EquatableBasicDigitalTwin> memoized = new();
            EquatableBasicDigitalTwin identityMap(BasicDigitalTwin twin) => memoized.GetOrAdd(twin.Id, new EquatableBasicDigitalTwin(twin));

            // Replace "all" at the end of the twinids list with the top-level nodes
            if (twinIds.Contains("all"))
            {
                string[] topLevelModels = new string[] {
                    "dtmi:com:willowinc:Portfolio;1",
                    "dtmi:com:willowinc:Building;1",
                    "dtmi:com:willowinc:Land;1", "dtmi:com:willowinc:Floor;1",
                    "dtmi:com:willowinc:mining:System;1",
                    "dtmi:com:willowinc:Equipment;1"
                };

                foreach (string topLevelModel in topLevelModels)
                {
                    var topLevelTwins = _azureDigitalTwinReader.QueryAsync<BasicDigitalTwin>($"SELECT $dtId FROM DIGITALTWINS DT WHERE IS_OF_MODEL(DT, '{topLevelModel}')");
                    var page = await topLevelTwins.AsPages().FirstAsync();
                    twinIds = twinIds.Where(x => x != "all").Concat(page.Values.Select(x => x.Id)).ToArray();
                }
            }

            HashSet<string> seen = new();
            Queue<(string twinId, int distance, string following)> queue = new();
            foreach (var twinId in twinIds)
            {
                queue.Enqueue((twinId, 0, ""));
            }

            int limit = 10000;

            while (queue.Count > 0)
            {
                if (limit-- < 0) { _logger.LogWarning("Graph traversal hit traversals limit"); break; }

                (string id, int distance, string following) = queue.Dequeue();
                if (seen.Contains(id)) continue;
                seen.Add(id);

                bool isInitialNode = distance == 0;

                var twin = identityMap(await _azureDigitalTwinReader.GetDigitalTwinAsync(id));

                switch (following)
                {
                    case "feeds":
                        {
                            var relatedBackTwins = await _azureDigitalTwinReader.GetTwinRelationshipsAsync(twin.Id);
                            foreach (var edge in relatedBackTwins.Where(x => x.Name == "isFedBy"))
                            {
                                var pred = TwinRelation.Get("feeds");
                                result.AddStatement(identityMap(await _azureDigitalTwinReader.GetDigitalTwinAsync(edge.TargetId)), pred, twin);

                                queue.Enqueue((edge.TargetId, distance + 1, "feeds"));
                            }
                            break;
                        }
                    case "isFedBy":
                        {
                            var relatedForwardTwins = await _azureDigitalTwinReader.GetIncomingRelationshipsAsync(twin.Id);
                            foreach (var edge in relatedForwardTwins.Where(x => x.Name == "isFedBy"))
                            {
                                var pred = TwinRelation.Get("feeds");
                                result.AddStatement(twin, pred, identityMap(await _azureDigitalTwinReader.GetDigitalTwinAsync(edge.SourceId)));

                                queue.Enqueue((edge.SourceId, distance + 1, "isFedBy"));
                            }
                            break;
                        }
                    case "physicalGreater":
                        {
                            var relatedBackTwins = await _azureDigitalTwinReader.GetTwinRelationshipsAsync(twin.Id);
                            foreach (var edge in relatedBackTwins)
                            {
                                if (!"locatedIn|isPartOf|includedIn".Contains(edge.Name)) continue;

                                var pred = TwinRelation.Get(edge.Name);
                                result.AddStatement(identityMap(await _azureDigitalTwinReader.GetDigitalTwinAsync(edge.TargetId)), pred, twin);

                                queue.Enqueue((edge.TargetId, distance + 1, "physicalGreater"));
                            }
                            break;
                        }
                    case "isCapabilityOf":
                    default:
                        {
                            //if (isInitialNode)
                            {
                                var relatedForwardTwins = (await _azureDigitalTwinReader.GetTwinRelationshipsAsync(twin.Id)).ToList();
                                foreach (var edge in relatedForwardTwins)
                                {
                                    var destination = await _azureDigitalTwinReader.GetDigitalTwinAsync(edge.TargetId);
                                    if (edge.Name == "isPartOf" || edge.Name == "locatedIn")
                                    {
                                        // If the parent is an HVACZone we are interested in what feeds that zone
                                        // so push in on the queue with empty string to examine all of its links
                                        if (destination.Metadata.ModelId == "dtmi:com:willowinc:HVACZone;1")
                                        {
                                            queue.Enqueue((destination.Id, distance + 1, ""));
                                        }
                                        else if (destination.Metadata.ModelId == "dtmi:com:willowinc:OccupancyZone;1")
                                        {
                                            // These also act like a zone
                                            queue.Enqueue((destination.Id, distance + 1, ""));
                                        }
                                        else if (destination.Metadata.ModelId == "dtmi:com:willowinc:InferredOccupancySensor;1")
                                        {
                                            // These also act like a zone
                                            queue.Enqueue((destination.Id, distance + 1, ""));
                                        }
                                        else
                                        {
                                            queue.Enqueue((destination.Id, distance + 1, "physicalGreater"));
                                        }

                                        var pred = TwinRelation.Get(edge.Name);
                                        result.AddStatement(twin, pred, identityMap(destination));
                                    }
                                    else if (edge.Name == "isFedBy")
                                    {
                                        queue.Enqueue((destination.Id, distance + 1, "feeds"));
                                        // flip the edge for the output
                                        var pred = TwinRelation.Get("feeds");
                                        result.AddStatement(identityMap(destination), pred, twin);
                                    }
                                    else if (edge.Name == "isCapabilityOf")
                                    {
                                        queue.Enqueue((destination.Id, distance + 1, "isCapabilityOf"));

                                        var pred = TwinRelation.Get(edge.Name);
                                        result.AddStatement(twin, pred, identityMap(destination));
                                    }
                                    else if (edge.Name == "hostedBy" && following == "isCapabilityOf")
                                    {
                                        // If we came in on isCapabilityOf, don't leave on hostedBy
                                    }
                                }

                                var relatedBackTwins = (await _azureDigitalTwinReader.GetIncomingRelationshipsAsync(twin.Id)).ToList();
                                foreach (var edge in relatedBackTwins)
                                {
                                    var destination = await _azureDigitalTwinReader.GetDigitalTwinAsync(edge.SourceId);
                                    if (edge.Name == "isFedBy")
                                    {
                                        // flip the edge for the output
                                        var pred = TwinRelation.Get("feeds");
                                        result.AddStatement(identityMap(destination), pred, twin);

                                        // Bounce back down through an HVACZone
                                        if (destination.Metadata.ModelId == "dtmi:com:willowinc:HVACZone;1")
                                        {
                                            queue.Enqueue((destination.Id, distance + 1, ""));
                                        }
                                        else if (destination.Metadata.ModelId == "dtmi:com:willowinc:OccupancyZone;1")
                                        {
                                            // These also act like a zone
                                            queue.Enqueue((destination.Id, distance + 1, ""));
                                        }
                                        else if (destination.Metadata.ModelId == "dtmi:com:willowinc:InferredOccupancySensor;1")
                                        {
                                            // These also act like a zone
                                            queue.Enqueue((destination.Id, distance + 1, ""));
                                        }
                                        else
                                        {
                                            queue.Enqueue((destination.Id, distance + 1, "isFedBy"));
                                        }

                                    }
                                    else if (edge.Name == "isCapabilityOf")
                                    {
                                        // If we go down to a capability, also look up from that capability
                                        // because of the double isCapabilityOf issue around HasInferredOccupancy and People Count
                                        queue.Enqueue((destination.Id, distance + 1, "isCapabilityOf"));
                                        // Mostly this will get straight back to the same node, but sometimes ...

                                        var pred = TwinRelation.Get(edge.Name);
                                        result.AddStatement(identityMap(destination), pred, twin);
                                    }
                                    else if (edge.Name == "isPartOf")
                                    {
                                        if (twin.Metadata.ModelId == "dtmi:com:willowinc:OccupancyZone;1")
                                        {
                                            // check it's an inferred occupancy
                                            queue.Enqueue((destination.Id, distance + 1, ""));
                                        }
                                        var pred = TwinRelation.Get(edge.Name);
                                        result.AddStatement(identityMap(destination), pred, twin);
                                    }
                                    else
                                    {
                                        // Something else, add it to graph but don't follow it
                                        var pred = TwinRelation.Get(edge.Name);
                                        result.AddStatement(identityMap(destination), pred, twin);
                                    }
                                }
                            }
                            break;
                        }
                }
            }

            return result;
        }

        // We could add a forceRefreshTwinCountCache bool to GetTwinInfoRequest if we want to
        //  give the caller the option to get real-time twin counts
        public async Task<Dictionary<string, int>> GetTwinCountByModelAsync(IEnumerable<string> modelIds, string locationId, SourceType sourceType, bool forceRefreshCache = false)
        {
            var models = !modelIds.Any() ? "All" : string.Join("_", modelIds);
            var thisLocationId = string.IsNullOrEmpty(locationId) ? "All" : locationId;
            var exactModelMatch = "Match"; //Default value for now
            var source = (SourceType)Enum.Parse(typeof(SourceType), sourceType.ToString());

            string key = $"{source}-twin-count-{models}-{thisLocationId}-{exactModelMatch}";

            if (forceRefreshCache)
            {
                _memoryCache.Remove(key);
            }

            switch (sourceType)
            {
                case SourceType.Adx:
                    return await CacheTwinsCountResponse(
                         key,
                         () => _adxService.GetTwinCountByModelAsync(locationId));// Ignoring the passed in models for ADX because so far its efficient enough to
                                                                                 // summarize count all the models - if we decide we need to query
                                                                                 // only a subset, we need to deal with the single IN query getting too large
                                                                                 // if many models are specified.

                case SourceType.AdtQuery:
                    return await CacheTwinsCountResponse(
                       key,
                       () => BuildADTTwinsCount(modelIds, locationId));
                default:
                    return null;
            }
        }

        public async Task<bool> TryDeleteDocFromCopilot(string blobName)
        {
            if (_copilotClient is null)
            {
                _logger.LogWarning("DeleteDocFromCopilot: Copilot client not available");
                return false;
            }

            try
            {
                _ = await _copilotClient.DeleteDocAsync(new DeleteDocumentRequest { Blob_file = blobName });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteDocFromCopilot: Failed to delete document {blobUri}", blobName);
                return false;
            }
        }

        private async Task<MultipleEntityResponse> DeleteTwinAndRelationships(
            string id,
            bool deleteRelationships,
            MultipleEntityResponse resp = null)
        {
            var recursing = resp is not null;
            resp ??= new MultipleEntityResponse();
            bool? deleted = null;

            _logger.LogWarning("DeleteTwin: Attempting to delete twin '{twinId}' (second attempt: {recurse}", id, recursing);

            try
            {
                var twin = await GetDocumentTwin(id);
                if (twin is not null && twin.Contents.TryGetValue("url", out var url))
                    _ = await TryDeleteDocFromCopilot(url.ToString().Split('/').Last());

                // Attempt to delete twin from ADT
                using (_telemetryCollector.StartActivity("DeleteTwinAndRelationships", ActivityKind.Consumer))
                    await _azureDigitalTwinWriter.DeleteDigitalTwinAsync(id);
                resp.Add(HttpStatusCode.OK, id, null, "DeleteTwin", msg: null);
                deleted = true;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                deleted = false;
                _logger.LogWarning("DeleteTwin: Twin '{twinId}' not found in ADT", id);
                resp.Add(HttpStatusCode.NotFound, id, null, "DeleteTwin", ex);
                // If twin is not found in ADT but it was requested to be deleted, this is probably because
                //   ADX is out-of-sync w/ADT for some reason and
                //    it was returned from ADX,  so we still want to "delete" it from ADX, below.
            }
            catch (Azure.RequestFailedException ex)
                when (ex.Status == (int)HttpStatusCode.BadRequest && ex.ErrorCode == "RelationshipsNotDeleted")
            {
                if (recursing)
                {
                    var newEx = new Azure.RequestFailedException("DeleteTwinAndRelationships: Twin still reports relationships after deleting ADT relationships", ex);
                    return resp.Add(HttpStatusCode.InternalServerError, id, null, "DeleteTwin", ex);
                }

                _logger.LogWarning("DeleteTwin: Twin '{twinId}' has relationships (deleteRels:{dr}", id, deleteRelationships);

                if (!deleteRelationships)
                    return resp.Add(HttpStatusCode.BadRequest, id, null, "DeleteTwin", ex);

                await DeleteRelationships(id, resp);

                // Recursive call to try deleting twins again, now that relationships has been deleted
                await DeleteTwinAndRelationships(id, deleteRelationships, resp);

                return resp;
            }
            catch (Exception ex)
            {
                return resp.Add(HttpStatusCode.FailedDependency, id, null, "DeleteTwin", ex);
            }

            if (deleted == true)
            {
                // We found the twin in ADT, therefore should already be sending an event on the service-bus which
                //   we should then process in our background worker to add the deletion record to ADX
                return resp;
            }

            // Add marked-for-deletion record to ADX for the twin
            // There's no need to spend the extra time to read ADX to check for existence first --
            //   whether the twin was never in ADX or was already deleted,, the MFD record will just be a no-op.
            _logger.LogInformation("DeleteTwin: Twin {twinId} adding deletion record to ADX", id);
            try
            {
                // We are just marking the twin for deletion so the materialized view doesn't include the MFD'd twin,
                //   so there is no need to fetch it first to merge in the properties of the full twin.
                // var twin = await _azureDigitalTwinReader.GetDigitalTwinAsync(id);
                var staleADXTwin = await _adxService.GetTwinsByIds([id], false);
                if (staleADXTwin.Content.Any())
                {
                    await _exportService.AppendTwinToAdx(staleADXTwin.Content.First().Twin, flagDelete: true);
                    _telemetryCollector.TrackADXTwinDeletionCount(1);
                }

            }
            catch (Exception ex)
            {
                // Note we are currently eating these errors as not to expose ADX hackery to the client - these deletes are
                //   precautionary only, as the twin was not in ADT and should not be in ADX either
                _logger.LogError(ex, "DeleteTwin: Unable to add deletion record for twin {id}  in ADX", id);

            }

            return resp;
        }

        private async Task DeleteRelationships(string id, MultipleEntityResponse resp)
        {
            IEnumerable<BasicRelationship> inRels = null, outRels = null;

            (_, outRels) = await resp.ExecuteAsync(id,
                async () => await _azureDigitalTwinReader.GetTwinRelationshipsAsync(id),
                "GetRelationships", null, _logger, onlyAddErrors: true);

            (_, inRels) = await resp.ExecuteAsync(id,
                async () => await _azureDigitalTwinReader.GetIncomingRelationshipsAsync(id),
                "GetIncomingRelationships", null, _logger, onlyAddErrors: true);

            var allRels = Enumerable.Concat(
                outRels ?? Enumerable.Empty<BasicRelationship>(),
                inRels ?? Enumerable.Empty<BasicRelationship>()).ToList();

            // Note: onlyAddErrors is not set here, so the consumer may want to filter for operation=DeleteTwin
            using (_telemetryCollector.StartActivity("DeleteRelationships", ActivityKind.Consumer))
                await Parallel.ForEachAsync(allRels,
                async (rel, _) => await resp.ExecuteAsync(id,
                    async () => await _azureDigitalTwinWriter.DeleteRelationshipAsync(rel.SourceId, rel.Id),
                    "DeleteRelationship", rel.Id, _logger)); ;


            // Only force MFD record to ADX for relationships that were not found in ADT
            var failedRels = resp.Responses.Where(r =>
                r.Operation == "DeleteRelationship" && r.StatusCode == HttpStatusCode.NotFound).Select(r => r.EntityId);

            await Parallel.ForEachAsync(failedRels, async (rel, _) =>
            {
                try
                {
                    var minimalRelationship = new BasicRelationship { Id = rel };
                    await _exportService.AppendRelationshipToAdx(minimalRelationship, flagDelete: true);
                }
                catch (Exception ex)
                {
                    // Note we are currently eating these errors as not to expose ADX hackery to the client - these deletes are
                    //   precautionary only, as the twin was not in ADT and should not be in ADX either
                    _logger.LogError(ex, "DeleteTwin: Unable to add deletion record for twin {id}  in ADX", id);
                }
            });
        }

        private async Task<IEnumerable<BasicDigitalTwin>> FollowAllRelsToTargetModel(string twinId, IEnumerable<string> relNames, string[] toModels, bool modelExact, int size)
        {
            var frontier = new Stack<string>();
            var seen = new HashSet<string>();  // avoid graph loops
            var results = new List<BasicDigitalTwin>();

            var relTypes = relNames.Any() ? relNames : RelationshipTypes.GetLocationSearchRelTypes();
            int counter = 0;
            frontier.Push(twinId);
            while (frontier.Count > 0 && results.Count < size)
            {
                if (frontier.Count > 1000)
                {
                    // This should never happen with the "seen" list, but better safe than sorry
                    throw new InvalidOperationException("Depth-limit exceeded for relationship query");
                }
                var candidate = frontier.Pop();
                seen.Add(candidate);
                var twin = await _azureDigitalTwinReader.GetDigitalTwinAsync(candidate);
                if (IsMatchedModel(twin.Metadata.ModelId, toModels, modelExact))
                {
                    results.Add(twin);
                    continue;
                }

                var found = new ConcurrentBag<string>();
                var outgoing = async () =>
                {
                    var relations = await _azureDigitalTwinReader.GetTwinRelationshipsAsync(candidate);
                    foreach (var rel in relations.Where(r => relTypes.Contains(r.Name)))
                        found.Add(rel.TargetId);
                };

                var incoming = async () =>
                {
                    var relations = await _azureDigitalTwinReader.GetIncomingRelationshipsAsync(candidate);
                    foreach (var rel in relations.Where(r => relTypes.Contains(r.Name)))
                        found.Add(rel.SourceId);
                };

                await Task.WhenAll(outgoing(), incoming());



                foreach (var id in found)
                    if (!seen.Contains(id) && !frontier.Contains(id))
                        frontier.Push(id);
                counter++;
            }

            return results.ToArray();
        }

        private bool IsMatchedModel(string twinModel, string[] matchModels, bool modelExact)
        {
            if (matchModels == null || !matchModels.Any())
                return true;

            if (modelExact && matchModels.Contains(twinModel))
                return true;

            if (!modelExact && _azureDigitalTwinModelParser.IsDescendantOfAny(matchModels, twinModel))
                return true;

            return false;
        }

        private async Task PresetUniqueIdAndTrendId(BasicDigitalTwin twin)
        {
            Task<Page<BasicDigitalTwin>> getTwinTask = null;
            async Task<BasicDigitalTwin> existingTwinFunc()
            {
                // Reuse the getTwinTask while querying the existing twin from ADT,
                // so all calls to the existingTwinFunc awaits the same task

                getTwinTask ??= _azureDigitalTwinReader.GetTwinsAsync(new GetTwinsInfoRequest() { SourceType = SourceType.AdtQuery }, new string[] { twin.Id }, pageSize: 1);
                var getTwinResult = await getTwinTask;
                return getTwinResult.Content.FirstOrDefault();
            }

            var uniqueId = await _customColumnService.GetUniqueIdForTwin("uniqueID", twin, existingTwinFunc);
            if (uniqueId is not null)
            {
                twin.Contents["uniqueID"] = uniqueId;
            }

            var trendID = await _customColumnService.GetTrendIdForTwin("trendID", twin, existingTwinFunc);
            // if the trendID is null, then the twin is not a capability twin
            if (trendID is not null)
            {
                twin.Contents["trendID"] = trendID;
            }
        }

        private async Task FollowRelationshipsAsync(IEnumerable<string> relationshipsToFollow,
            List<NestedTwin> processQueue,
            ConcurrentBag<string> seen,
            ConcurrentBag<NestedTwin> newTwinsToProcess,
            Func<BasicRelationship, string> getTwinId,
            Func<Task<IEnumerable<BasicRelationship>>> getRelationships,
            Action<string> processParent = null)
        {
            if (relationshipsToFollow == null || !relationshipsToFollow.Any())
                return;

            var relationships = await getRelationships();
            var enqueueTwins = relationships
                .Where(x => relationshipsToFollow.Contains(x.Name))
                .Select(async x =>
                {
                    var nextTwinId = getTwinId(x);
                    if (processQueue.All(q => q.Twin.Id != nextTwinId) && !seen.Contains(nextTwinId))
                    {
                        var twin = await _azureDigitalTwinReader.GetDigitalTwinAsync(nextTwinId);
                        newTwinsToProcess.Add(new NestedTwin(twin));
                    }

                    if (processParent != null)
                        processParent(nextTwinId);
                });

            await Task.WhenAll(enqueueTwins);
        }

        // ADX aggregations are fast, but for ADT we need to do a separate query for each model in the Ontology.
        // Even in parallel, this usually times out the HTTP request.
        // We add a cache here so at least on the second attempt, there will be enough model counts cached to succeed.
        // Note this only gets called by TLM when ?source=ADT
        private async Task<Dictionary<string, int>> BuildADTTwinsCount(IEnumerable<string> modelIds, string locationId)
        {
            var concurrentDict = new ConcurrentDictionary<string, int>();

            await Parallel.ForEachAsync(modelIds, async (modelId, _) =>
            {
                var count = await _azureDigitalTwinReader.GetTwinsCountAsyncWithSearch(new GetTwinsInfoRequest { ModelId = new string[] { modelId }, LocationId = locationId });
                concurrentDict.TryAdd(modelId, count);
            });
            return new Dictionary<string, int>(concurrentDict);
        }

        private async Task<Dictionary<string, int>> CacheTwinsCountResponse(string key, Func<Task<Dictionary<string, int>>> getTwinCount)
        {
            return await _memoryCache.GetOrCreateAsync(key, async entry =>
            {
                entry.SetAbsoluteExpiration(CacheTimeoutModelCount);
                return await getTwinCount();
            });
        }
    }
}

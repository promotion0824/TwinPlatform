using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DigitalTwinCore.Extensions;
using DigitalTwinCore.Features.TwinsSearch.Dtos;
using DigitalTwinCore.Features.TwinsSearch.Models;
using DigitalTwinCore.Infrastructure.Extensions;
using DigitalTwinCore.Services;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Services.Adx;
using Willow.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace DigitalTwinCore.Features.TwinsSearch.Services
{
    public interface ISearchService
    {
        Task<SearchResponse> Search(SearchRequest request, CancellationToken cancellationToken);
        Task<IEnumerable<SearchTwin>> BulkQuery(BulkQueryRequest request, CancellationToken cancellationToken);
        Task<IEnumerable<SearchTwin>> GetCognitiveSearchTwins(CognitiveSearchRequest request, CancellationToken cancellationToken);
    }

    public class SearchService : ISearchService
    {
        private readonly IAdxHelper _adxHelper;
        private readonly ISiteAdtSettingsProvider _siteAdtSettingsProvider;
        private readonly IDigitalTwinServiceProvider _digitalTwinServiceProvider;
        private readonly ILogger<SearchService> _logger;

        public SearchService(IAdxHelper adxHelper,
            ISiteAdtSettingsProvider siteAdtSettingsProvider,
            IDigitalTwinServiceProvider digitalTwinServiceProvider,
            ILogger<SearchService> logger)
        {
            _adxHelper = adxHelper;
            _siteAdtSettingsProvider = siteAdtSettingsProvider;
            _digitalTwinServiceProvider = digitalTwinServiceProvider;
            _logger = logger;
        }

        public async Task<SearchResponse> Search(SearchRequest request, CancellationToken cancellationToken)
        {
            var result = await GetTwins(request, cancellationToken);

            result.searchResponse.Twins = await LoadRelationships(result.searchResponse.Twins, result.databases, cancellationToken);

            //To include SiteRelationship in the OutRelationships of the Twin
            result.searchResponse.Twins = await LoadSiteRelationship(result.searchResponse.Twins, result.databases, cancellationToken);

            return result.searchResponse;
        }

        public async Task<IEnumerable<SearchTwin>> BulkQuery(BulkQueryRequest request, CancellationToken cancellationToken)
        {
            var siteSettings = await _siteAdtSettingsProvider.GetForSitesAsync(request.SiteIds);
            var databases = siteSettings.GetDatabases().ToArray();
            var availableSiteIds = siteSettings.GetSites().ToArray();

            var query = SearchQueries.BuildBulkQuery(request.QueryId, availableSiteIds, request.Twins);

            using var reader = await _adxHelper.Query(databases.First(), query, cancellationToken);
            var twins = reader.Parse<SearchTwin>().ToArray();

            return twins;
        }

        private async Task<SearchTwin[]> LoadRelationships(SearchTwin[] twins, string[] databases, CancellationToken cancellationToken)
        {
            if (!twins.Any())
            {
                return twins;
            }

            var inRelationshipsQuery = SearchQueries.GetInRelationshipsQuery(twins.Select(x => x.Id), databases);
            var outRelationshipsQuery = SearchQueries.GetOutRelationshipsQuery(twins.Select(x => x.Id), databases);

            var inRelationships = await QueryRelationships(databases.First(), inRelationshipsQuery, cancellationToken);
            var outRelationships = await QueryRelationships(databases.First(), outRelationshipsQuery, cancellationToken);

            foreach (var twin in twins)
            {
                twin.InRelationships = inRelationships.Where(x => x.TargetId == twin.Id);
                twin.OutRelationships = outRelationships.Where(x => x.SourceId == twin.Id);
            }

            return twins;
        }

        private async Task<SearchRelationship[]> QueryRelationships(string database, string query, CancellationToken cancellationToken)
        {
            using var reader = await _adxHelper.Query(database, query, cancellationToken);
            return reader.Parse<SearchRelationship>().ToArray();
        }

        private async Task<SearchResponse> ExecuteNewQuery(
            Guid[] siteIds,
            SearchRequest request,
            string[] databases,
            CancellationToken cancellationToken)
        {
            var models = await GetModelIds(siteIds, request.CategoryId, request.ModelId).ToArrayAsync(cancellationToken);

            string[] isCapabilityOfModelIds = null;

            if (request.IsCapabilityOfModelId != null) {
                // All the sites should have the same ontology so we can just pick the first one.
                var digitalTwinService = await _digitalTwinServiceProvider.GetForSiteAsync(siteIds[0]);
                var modelParser = await digitalTwinService.GetModelParserAsync();
                isCapabilityOfModelIds = modelParser
                    .GetInterfaceDescendants(new[] { request.IsCapabilityOfModelId })
                    .Values
                    .Select(m => m.Id.ToString())
                    .ToArray();
                if (isCapabilityOfModelIds.Length == 0)
                {
                    throw new ResourceNotFoundException("Model", request.IsCapabilityOfModelId);
                }
            }

            var (queryId, query) = SearchQueries.BuildFirstPageQuery(
                siteIds,
                request,
                models.ToArray(),
                isCapabilityOfModelIds,
                databases);

            using var reader = await _adxHelper.Query(databases.First(), query, cancellationToken);
            var totalCount = await GetTotalCount(queryId, databases.First(), cancellationToken);
            var response = new SearchResponse
            {
                QueryId = queryId,
                NextPage = request.PageSize < totalCount ? 1 : -1,
                Twins = reader.Parse<SearchTwin>().ToArray()
            };

            return response;
        }

        private async Task<SearchResponse> FetchPreviousQuery(IEnumerable<Guid> siteIds, SearchRequest request,
            IEnumerable<string> databases, CancellationToken cancellationToken)
        {
            var query = SearchQueries.BuildFollowUpQuery(siteIds, request);

            using var reader = await _adxHelper.Query(databases.First(), query, cancellationToken);
            var totalCount = await GetTotalCount(request.QueryId, databases.First(), cancellationToken);
            var page = request.Page + 1;
            var response = new SearchResponse
            {
                Twins = reader.Parse<SearchTwin>().ToArray(),
                QueryId = request.QueryId,
                NextPage = (page * request.PageSize) < totalCount ? page : -1
            };

            return response;
        }

        private IAsyncEnumerable<KeyValuePair<Guid, IEnumerable<string>>> GetModelIds(IEnumerable<Guid> siteIds, Guid? categoryId, string modelId)
        {
            return _digitalTwinServiceProvider.GetModelIds(siteIds,
                                                                categoryId.HasValue ? new Guid[] { categoryId.Value } : Array.Empty<Guid>(),
                                                                modelId,
                                                                true);
        }

        private async Task<long> GetTotalCount(string queryId, string database, CancellationToken cancellationToken)
        {
            var storedQueryCount = SearchQueries.BuildStoredQueryCount(queryId);

            using var countReader = await _adxHelper.Query(database, storedQueryCount, cancellationToken);

            return countReader.Parse<SearchTwinCount>().First().Count;
        }

        private async Task<IEnumerable<SearchTwin>> GetActiveTwins(IEnumerable<string> databases, IEnumerable<Guid> siteIds, CancellationToken cancellationToken)
        {
            var query = SearchQueries.GetActiveTwinsQuery(databases, siteIds);
            using var reader = await _adxHelper.Query(databases.First(), query, cancellationToken);
            var twins = reader.Parse<SearchTwin>().ToArray();
            return twins;
        }

        private async Task<SearchTwin[]> LoadSiteRelationship(SearchTwin[] twins, string[] databases, CancellationToken cancellationToken)
        {
            if (!twins.Any())
            {
                return twins;
            }

            var siteTwins = await GetActiveTwins(databases, twins.DistinctBy(x => x.SiteId).Select(x => x.SiteId), cancellationToken);
            foreach (var twin in twins)
            {
                var searchRelationship = new SearchRelationship();
                var siteTwin = siteTwins.Where(x => x.UniqueId == twin.SiteId).FirstOrDefault();
                searchRelationship =
                    new SearchRelationship()
                    {
                        Name = "locatedIn",
                        SourceId = twin.Id,
                        TargetId = siteTwin?.Id
                    };

                var relationships = twin.OutRelationships.Concat(twin.InRelationships).ToList();

                if (twin.Id == searchRelationship.TargetId || relationships.Any(r => r.TargetId == searchRelationship.TargetId))
                {
                    continue;
                }

                var outRelationships = new List<SearchRelationship>();
                outRelationships = twin.OutRelationships.ToList();
                outRelationships.Add(searchRelationship);
                twin.OutRelationships = outRelationships;
            }

            return twins;
        }

        private async Task<(SearchResponse searchResponse, string[] databases)> GetTwins(SearchRequest request, CancellationToken cancellationToken)
        {
            var siteSettings = await _siteAdtSettingsProvider.GetForSitesAsync(request.SiteIds);
            var databases = siteSettings.GetDatabases().ToArray();
            var availableSiteIds = siteSettings.GetSites().ToArray();

            if (!databases.Any())
            {
                return (null, null);
            }
            SearchResponse response = null;

            if (string.IsNullOrWhiteSpace(request.QueryId))
            {
                response = await ExecuteNewQuery(availableSiteIds, request, databases, cancellationToken);
            }
            else
            {
                response = await FetchPreviousQuery(availableSiteIds, request, databases, cancellationToken);
            }

            return (response, databases);
        }

        public async Task<IEnumerable<SearchTwin>> GetCognitiveSearchTwins(CognitiveSearchRequest request, CancellationToken cancellationToken)
        {
            var siteSettings = await _siteAdtSettingsProvider.GetForSitesAsync(request.SiteIds);
            var databases = siteSettings.GetDatabases().ToArray();

            if (!databases.Any())
            {
                return new List<SearchTwin>();
            }

            var query = SearchQueries.CognitiveSearchQuery(request.TwinIds, databases);

            using var reader = await _adxHelper.Query(databases.First(), query, cancellationToken);
            var twins = reader.Parse<SearchTwin>().ToArray();

            if (request.SensorSearchEnabled)
            {
                twins = await LoadRelationships(twins, databases, cancellationToken);
            }

            return twins;
        }
    }
}

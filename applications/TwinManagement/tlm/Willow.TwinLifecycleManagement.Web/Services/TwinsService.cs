using Azure.DigitalTwins.Core;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.Batch;
using Willow.TwinLifecycleManagement.Web.Models;
using MultipleEntityResponse = Willow.AzureDigitalTwins.SDK.Client.MultipleEntityResponse;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    public class TwinsService(IImportClient importClient, ITwinsClient twinsClient) : ITwinsService
    {

        public async Task<Page<TwinWithRelationships>> GetTwinsWithRelationshipsAsync(
            string locationId = null,
            string[] modelIds = null,
            bool exactModelMatch = false,
            int pageSize = 100,
            string searchString = "",
            string continuationToken = null,
            bool includeRelationships = false,
            bool includeIncomingRelationships = false,
            SourceType sourceType = SourceType.Adx,
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null,
            bool includeTotalCount = false)
        {
            var getTwinsInfoRequest = new GetTwinsInfoRequest
            {
                ModelId = modelIds,
                LocationId = locationId,
                ExactModelMatch = exactModelMatch,
                IncludeRelationships = includeRelationships,
                IncludeIncomingRelationships = includeIncomingRelationships,
                SourceType = sourceType,
                RelationshipsToTraverse = [],
                SearchString = searchString,
                StartTime = startTime,
                EndTime = endTime,
            };

            var twinsWithRelationships = await twinsClient.GetTwinsAsync(
                                                                        getTwinsInfoRequest,
                                                                        pageSize,
                                                                        includeTotalCount,
                                                                        continuationToken);

            return twinsWithRelationships;
        }

        public async Task<Page<TwinWithRelationships>> QueryTwinsWithRelationshipsAsync(
            GetTwinsInfoRequestBFF request,
            int pageSize = 100,
            string continuationToken = null,
            bool includeTotalCount = false)
        {
            var whereExpression = request.BuildWhereExpression();

            if (whereExpression != string.Empty)
            {
                request.QueryFilter.Filter = whereExpression;
            }

            return await twinsClient.QueryTwinsAsync(request, pageSize, continuationToken, includeTotalCount);
        }

        public async Task<IEnumerable<NestedTwin>> GetTwinsTreeAsync(
            string[] modelIds = null,
            string[] outgoingRelationships = null,
            string[] incomingRelationships = null,
            bool exactModelMatch = false)
        {
            var tree = await twinsClient.GetTreesByModelAsync(modelIds, [], outgoingRelationships, incomingRelationships, exactModelMatch);

            return tree;
        }

        public async Task<MultipleEntityResponse> DeleteTwins(string[] twinIds = null, bool deleteRelationships = false)
        {
            var multiResponse = await twinsClient.DeleteTwinsAndRelationshipsAsync(twinIds, deleteRelationships);

            return multiResponse;

        }

        public async Task<int> GetTwinsCount(
            string locationId = null,
            string[] modelIds = null,
            bool exactModelMatch = false,
            SourceType sourceType = SourceType.Adx,
            string searchString = null,
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null)
        {
            return await twinsClient.GetTwinsCountAsync(
                                                            modelIds,
                                                            locationId,
                                                            exactModelMatch,
                                                            includeRelationships: false,
                                                            includeIncomingRelationships: false,
                                                            orphanOnly: false,
                                                            sourceType,
                                                            relationshipsToTraverse: null,
                                                            searchString,
                                                            startTime,
                                                            endTime);

        }

        public async Task<IEnumerable<TwinWithRelationships>> GetAllTwinsAsync(
            string locationId = null,
            string[] modelIds = null,
            bool exactModelMatch = false,
            bool includeRelationships = false,
            bool includeIncomingRelationships = false,
            SourceType sourceType = SourceType.Adx)
        {
            List<TwinWithRelationships> twins = new();
            string continuationToken = null;
            do
            {
                var response = await GetTwinsWithRelationshipsAsync(
                                                                    locationId,
                                                                    modelIds,
                                                                    exactModelMatch,
                                                                    pageSize: 10000,
                                                                    searchString: string.Empty,
                                                                    continuationToken,
                                                                    includeRelationships,
                                                                    includeIncomingRelationships,
                                                                    sourceType: sourceType);
                if (response.Content.Any())
                {
                    twins.AddRange(response.Content.ToList());
                }

                continuationToken = response.ContinuationToken;
            }
            while (continuationToken is not null);

            return twins;
        }

        public async Task<TwinWithRelationships> GetTwinAsync(string twinId, SourceType sourceType, bool includeRelationships)
        {
            var response = await twinsClient.GetTwinsByIdsAsync(new[] { twinId }, sourceType, includeRelationships);
            return response.Content.SingleOrDefault();
        }

        public async Task<IEnumerable<TwinWithRelationships>> GetTwinByIdsAsync(string[] twinIds, SourceType sourceType, bool includeRelationships)
        {
            var response = await twinsClient.GetTwinsByIdsAsync(twinIds, sourceType, includeRelationships);
            return response.Content;
        }

        public async Task<JobsEntry> PostTwinsAndRelationshipsAsync(BulkImportTwinsRequest twinsRequest, string userData)
        {
            return await importClient.TriggerTwinsImportAsync(twinsRequest, userData: userData);
        }

        public async Task PatchTwin(string twinId, IEnumerable<Operation> jsonPatch, bool includeAdxUpdate = false)
        {
            await twinsClient.PatchTwinAsync(twinId, jsonPatch, includeAdxUpdate);
        }

        public async Task<BasicDigitalTwin> PutTwin(BasicDigitalTwin twin)
        {
            return await twinsClient.UpdateTwinAsync(twin, includeAdxUpdate: true);
        }
    }

    public static class GetTwinsInfoRequestExtensions
    {
        public static string BuildWhereExpression(this GetTwinsInfoRequestBFF request)
        {
            var filters = new List<string>();

            var prependField = "twins.";

            foreach (var filter in request.FilterSpecifications)
            {
                if (request.SourceType == SourceType.AdtQuery)
                {
                    var expression = filter.BuildAdtQueryExpression($"{prependField}{filter.Field}");
                    filters.Add(expression);
                }
                else if (request.SourceType == SourceType.Adx)
                {
                    var expression = filter.BuildKqlExpression();
                    filters.Add(expression);
                }
                else
                {
                    throw new ArgumentException($"Filtering on source type {request.SourceType} is not implemented.");
                }
            }

            string whereClause = string.Empty;

            if (filters.Any())
            {
                var logicalOperator = "and";
                whereClause = $"{string.Join($" {logicalOperator} ", filters)}";
            }

            return whereClause;
        }
    }
}

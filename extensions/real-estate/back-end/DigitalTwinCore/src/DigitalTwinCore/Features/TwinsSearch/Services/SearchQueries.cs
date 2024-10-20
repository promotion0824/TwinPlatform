using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalTwinCore.Features.TwinsSearch.Dtos;
using DigitalTwinCore.Features.TwinsSearch.Models;
using DigitalTwinCore.Services.Adx;
using Diacritics.Extensions;

namespace DigitalTwinCore.Features.TwinsSearch.Services
{
    public static class SearchQueries
    {
        public static (string queryId, string query) BuildFirstPageQuery(
            IEnumerable<Guid> siteIds,
            SearchRequest request,
            KeyValuePair<Guid, IEnumerable<string>>[] siteModels,

            // If request.isCapabilityOfModelIds is provided, this should be the list
            // of model IDs that are descendants of that model ID (including that model
            // ID itself). In future we might want to refactor so that we can calculate
            // that from here.
            string[] isCapabilityOfModelIds,
            IEnumerable<string> databases)
        {
            var queryId = Guid.NewGuid().ToString("N");
            var queryBuilder = new StringBuilder($".set stored_query_result {queryId} with (previewCount = {request.PageSize}) <");
            var fileTypes = request.FileTypes;

            queryBuilder.AppendLine($"| {AdxExtensions.CrossDatabaseUnion(databases, AdxConstants.ActiveTwinsFunction)}");
            queryBuilder.AppendLine(AdxExtensions.SitesFilter(siteIds));
            if (!string.IsNullOrEmpty(request.Term))
            {
                if (request.Term.HasDiacritics())
                {
                    queryBuilder.AppendLine($"| where {nameof(SearchTwin.Name)} matches regex '(?i){request.Term.Trim().Escape()}'");
                }
                else
                {
                    queryBuilder.AppendLine($"| where {nameof(SearchTwin.Name)} contains '{request.Term.Trim().Escape()}'");
                }
            }
            if (fileTypes?.Any() == true)
            {
                queryBuilder.Append($"| where {nameof(SearchTwin.Name)} hassuffix '{fileTypes.First().Trim().Escape()}'");
                for (var i = 1; i < fileTypes.Length; i++)
                {
                    queryBuilder.Append($" or {nameof(SearchTwin.Name)} hassuffix '{fileTypes[i].Trim().Escape()}'");
                }
                queryBuilder.AppendLine();
            }
            if (siteModels.SelectMany(x => x.Value).Any())
            {
                queryBuilder.AppendLine(ModelsFilter(siteModels));
            }
            queryBuilder.AppendLine($"| order by {nameof(SearchTwin.Name)} asc");
            queryBuilder.AppendLine($"| project Num=row_number(), {nameof(SearchTwin.Id)}, {nameof(SearchTwin.Name)}, {nameof(SearchTwin.SiteId)}, {nameof(SearchTwin.FloorId)}, {nameof(SearchTwin.ModelId)}, {nameof(SearchTwin.UniqueId)}, {nameof(SearchTwin.ExternalId)}, {nameof(SearchTwin.Raw)}");

            if (request.IsCapabilityOfModelId != null)
            {
                // Return only twins that join with a twin of one of the specified models
                // via an isCapabilityOf relationship
				var modelIdsExpr = string.Join(", ", isCapabilityOfModelIds.Select(id => '"' + id.Escape() + '"'));
                queryBuilder.Append($@"
                    | join kind=inner (
                        Relationships
                        | join kind=inner (
                            ActiveTwins
                            | where ModelId in ({modelIdsExpr})
                        ) on $left.TargetId == $right.Id
                        | where Name == ""isCapabilityOf""
                    ) on $left.Id == $right.SourceId
                ");
            }

            var query = queryBuilder.ToString();
            return (queryId, query);
        }

        private static string ModelsFilter(IEnumerable<KeyValuePair<Guid, IEnumerable<string>>> siteModels)
        {
            // Since sites have already been filtered, so only need to filter the model ids
            var modelIds = siteModels.SelectMany(x => x.Value.Select(model => $"'{model.Escape()}'")).Distinct();
            return $"| where {nameof(SearchTwin.ModelId)} in ({string.Join(",", modelIds)})";
        }

        public static string BuildFollowUpQuery(IEnumerable<Guid> siteIds, SearchRequest request)
        {
            var startItem = request.Page * request.PageSize + 1;
            var endItem = (request.Page + 1) * request.PageSize;

            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine($"stored_query_result(\"{request.QueryId.Escape()}\")");
            queryBuilder.AppendLine(AdxExtensions.SitesFilter(siteIds));
            queryBuilder.AppendLine($"| where Num between({startItem} .. {endItem})");

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Build a bulk query from stored query result in ADX for specified query id
        /// </summary>
        /// <param name="queryId">Query id stored in ADX</param>
        /// <param name="siteIds">List of sites to be fetch a twins with</param>
        /// <param name="twins">List of site/twin pairs to be fetched.
        /// Twins with site ids that are not in siteIds param will not be returned.
        /// Empty value will return all the twins for a specified query id and sites. </param>
        /// <returns>Bulk query string</returns>
        public static string BuildBulkQuery(string queryId, IEnumerable<Guid> siteIds, ICollection<SiteTwinPair> twins)
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine($"stored_query_result(\"{queryId.Escape()}\")");
            queryBuilder.AppendLine(AdxExtensions.SitesFilter(siteIds));

            if (!twins.Any()) return queryBuilder.ToString();

            var groups = twins.GroupBy(x => x.SiteId);
            queryBuilder.AppendLine("| where");
            queryBuilder.AppendLine(
                string.Join(" or ",
                groups.Select(x =>
                    $"({nameof(SearchTwin.SiteId)} == '{x.Key}' and {nameof(SearchTwin.Id)} in ({string.Join(',', x.Select(z => $"'{z.TwinId.Escape()}'"))}))"
                ))
            );

            return queryBuilder.ToString();
        }

        public static string GetInRelationshipsQuery(IEnumerable<string> modelIds, IEnumerable<string> databases)
        {
            return GetRelationshipsQuery(modelIds, databases, nameof(SearchRelationship.TargetId));
        }

        public static string GetOutRelationshipsQuery(IEnumerable<string> modelIds, IEnumerable<string> databases)
        {
            return GetRelationshipsQuery(modelIds, databases, nameof(SearchRelationship.SourceId));
        }

        /// <summary>
        /// If `sourceOrTarget` is "SourceId", return an ADX query to find all
        /// the relationships with whose source is one of the specified twins
        /// in `twinIds`. Else if `sourceOrTarget` is "TargetId", find
        /// relationships whose target is one of the specified twins.
        ///
        /// The query will return all the columns in the Relationships table,
        /// and the Id, Name, ModelId and FloorId columns of the related twins.
        /// </summary>
        /// <param name="twinIds">dtIds to look for</param>
        /// <param name="databases">Names of databases to look in</param>
        /// <param name="sourceOrTarget">Should be "SourceId" or "TargetId" - determines
        ///   which end of the relationship to match with the twin IDs.
        /// </param>
        private static string GetRelationshipsQuery(IEnumerable<string> twinIds, IEnumerable<string> databases, string sourceOrTarget)
        {
            var otherEnd = sourceOrTarget == "SourceId" ? "TargetId" : "SourceId";
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine($"{AdxExtensions.CrossDatabaseUnion(databases, AdxConstants.ActiveRelationshipsFunction)}");
            queryBuilder.AppendLine($"| where {sourceOrTarget} in ({string.Join(",", twinIds.Select(x => $"'{x.Escape()}'"))})");
            queryBuilder.AppendLine($"| join kind=inner (ActiveTwins | project Id, TwinName=Name, ModelId, FloorId) on $left.{otherEnd} == $right.Id");
            return queryBuilder.ToString();
        }

        public static string BuildStoredQueryCount(string queryId)
        {
            return $"stored_query_result(\"{queryId}\") | count";
        }

        public static string GetActiveTwinsQuery(IEnumerable<string> databases, IEnumerable<Guid> siteIds)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine($"{AdxExtensions.CrossDatabaseUnion(databases, AdxConstants.ActiveTwinsFunction)}");
            queryBuilder.AppendLine($"| where {nameof(SearchTwin.UniqueId)} in ({string.Join(",", siteIds.Select(x => $"'{x}'"))})");
            return queryBuilder.ToString();
        }

        /// <summary>
        /// Build a query for cognitive search result in ADX
        /// </summary>
        /// <param name="twinIds">List of twin ids to be filtered</param>
        /// <param name="databases">List of databases to be joined</param>
        /// <returns>Query string</returns>
        public static string CognitiveSearchQuery(
            IEnumerable<string> twinIds,
            IEnumerable<string> databases)
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine($"{AdxExtensions.CrossDatabaseUnion(databases, AdxConstants.ActiveTwinsFunction)}");
            queryBuilder.AppendLine(AdxExtensions.TwinIdsFilter(twinIds));

            queryBuilder.AppendLine($"| order by {nameof(SearchTwin.Name)} asc");
            queryBuilder.AppendLine($"| project Num=row_number(), {nameof(SearchTwin.Id)}, {nameof(SearchTwin.Name)}, {nameof(SearchTwin.SiteId)}, {nameof(SearchTwin.FloorId)}, {nameof(SearchTwin.ModelId)}, {nameof(SearchTwin.UniqueId)}, {nameof(SearchTwin.ExternalId)}, {nameof(SearchTwin.Raw)}");

            var query = queryBuilder.ToString();
            return query;
        }
    }
}

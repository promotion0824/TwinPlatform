using System;
using System.Collections.Generic;
using System.Linq;
using Willow.IoTService.Monitoring.Models;
using Willow.IoTService.Monitoring.Queries;

namespace Willow.IoTService.Monitoring.Services.AppInsights
{
    public static class KqlFilterBuilder
    {
        const string where = " | where";
        const string and = "and";

        public static string AppendFilters(string kql, IEnumerable<IMetricQueryFilter>? queryFilters = null)
        {
            if (queryFilters == null || !queryFilters.Any())
            {
                return kql;
            }

            var kqlQueries = SplitUnionedKql(kql);

            var kqlWithFilters = kqlQueries.Select(q => AppendFiltersInternal(q, queryFilters));

            return string.Join(" | union | ", kqlWithFilters);
        }

        private static string AppendFiltersInternal(string kql, IEnumerable<IMetricQueryFilter> queryFilters)
        {
            var parts = kql.Split('|');

            var source = parts[0];

            var originalKqlFilters = kql[source.Length..];

            var filters = new List<string>();

            foreach (var filter in queryFilters)
            {
                var whereAnd = filters.Count == 0 ? $" {where} " : $" {and} ";

                var filterKql = GetKqlForFilter(filter);

                filters.Add($"{whereAnd} {filterKql}");
            }

            var whereClause = string.Concat(filters);

            if (!string.IsNullOrEmpty(originalKqlFilters) && originalKqlFilters.StartsWith(where, StringComparison.InvariantCultureIgnoreCase))
            {
                originalKqlFilters = $" {and} {originalKqlFilters.Substring(where.Length)}";
            }

            return $"{source} {whereClause} {originalKqlFilters}";
        }

        private static IEnumerable<string> SplitUnionedKql(string kql)
        {
            var queue = new Queue<string>(kql.Split('|'));

            var queries = new List<(int QueryNumber, string QueryPart)>();

            int i = 0;

            while (queue.TryDequeue(out var part))
            {
                if (part.Trim().Equals("union", StringComparison.InvariantCultureIgnoreCase))
                {
                    i++;
                    continue;
                }

                queries.Add((i, part));
            }

            return queries.GroupBy(q => q.QueryNumber).Select(q => string.Join(" | ", q.Select(x => x.QueryPart)));
        }

        private static string GetKqlForFilter(IMetricQueryFilter filter)
        {
            if (filter is SiteIdFilter siteIdFilter)
            {
                return $"customDimensions.SiteId == '{siteIdFilter.SiteId}'";
            }

            if (filter is ConnectorIdFilter appConfigIdFilter)
            {
                return $"customDimensions.AppConfigId == '{appConfigIdFilter.ConnectorId}'";
            }

            if (filter is TimeAgoFilter timeAgoFilter)
            {
                return $"timestamp >= ago({timeAgoFilter.TimeAgo})";
            }

            if (filter is CustomDimensionEqualityFilter customFilter)
            {
                return CustomDimensionFilterKqlBuilder.GetKql(customFilter);
            }

            if (filter is CustomerIdFilter customerIdFilter)
            {
                return $"customDimensions.CustomerId == '{customerIdFilter.CustomerId}'";
            }

            throw new NotSupportedException($"QueryFilter '{filter}' not supported");
        }
    }
}
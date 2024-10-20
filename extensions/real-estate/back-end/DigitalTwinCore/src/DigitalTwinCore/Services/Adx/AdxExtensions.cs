using DigitalTwinCore.Features.TwinsSearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Services.Adx
{
	public static class AdxExtensions
	{
		public static string Escape(this string input)
		{
			return input.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\"", "\\\"");
		}

		public static string CrossDatabaseUnion(IEnumerable<string> databases, string table)
		{
			return $"union {CrossDatabaseTable(databases, table)}";
		}

		public static string CrossDatabaseTable(IEnumerable<string> databases, string table)
		{
			return string.Join(", ", databases.Select(x => $"database('{x.Escape()}').{table}"));
		}

		public static string OrExpansion<T>(string name, IEnumerable<T> values)
		{
			return string.Join(" or ", values.Select(x => $"{name} == '{x}'"));
		}

		public static string SitesFilter(IEnumerable<Guid> siteIds)
		{
			return $"| where {nameof(SearchTwin.SiteId)} in ({string.Join(",", siteIds.Select(x => $"'{x}'"))})";
		}

        public static string TwinIdsFilter(IEnumerable<string> twinIds)
        {
            return $"| where {nameof(SearchTwin.Id)} in ({string.Join(",", twinIds.Select(x => $"'{x.Escape()}'"))})";
        }
    }
}

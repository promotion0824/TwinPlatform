using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Batch;
using Willow.Platform.Models;

namespace PlatformPortalXL.Features.Insights
{
    public static class InsightScopeFilterExtension
    {
        public static string GetScopeId(this FilterSpecificationDto[] filters)
        {
            return filters.FirstOrDefault(nameof(Insight.ScopeId), FilterOperators.EqualsLiteral)?.Value.ToString();
        }

        public static async Task<FilterSpecificationDto[]> UpsertScopeIdFilter(this FilterSpecificationDto[] filters, IEnumerable<Guid> siteIds)
        {
            var siteIdFilter = filters.FirstOrDefault(nameof(Insight.SiteId));
            if (siteIdFilter != null)
            {
                var sites = siteIds.Select(x => new Site() { Id = x });

                siteIds = sites.FilterBy(new List<FilterSpecificationDto>()
                    .Upsert(nameof(Site.Id), siteIdFilter.Value))
                    .Select(x => x.Id);
            }

            return filters.ReplaceFilter(nameof(Insight.ScopeId), nameof(Insight.SiteId), FilterOperators.ContainedIn, siteIds);
        }

        public static async Task<FilterSpecificationDto[]> UpsertScopeFIdilter(this Task<FilterSpecificationDto[]> filterTask, IEnumerable<Guid> siteIds)
        {
            var filters = await filterTask;
            return await filters.UpsertScopeIdFilter(siteIds);
        }
    }
}

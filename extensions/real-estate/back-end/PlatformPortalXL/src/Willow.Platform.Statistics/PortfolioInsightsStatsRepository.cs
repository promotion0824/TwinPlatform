using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Willow.Api.Client;
using Willow.Data;
using Willow.Platform.Models;

namespace Willow.Platform.Statistics
{
    public class PortfolioInsightsStatsRepository : IReadRepository<(Guid CustomerId, Guid PortfolioId), InsightsStats>
    {
        private readonly IRestApi _directoryCore;
        private readonly IRestApi _insightsCore;

        public PortfolioInsightsStatsRepository(IRestApi directoryCore, IRestApi insightsCore)
        {
            _directoryCore = directoryCore;
            _insightsCore = insightsCore;
        }

        public async Task<InsightsStats> Get((Guid CustomerId, Guid PortfolioId) id)
        {
            // First get list of all sites in portfolio
            var sites = await _directoryCore.Get<List<Site>>($"customers/{id.CustomerId}/portfolios/{id.PortfolioId}/sites");

            // Then get all site stats
            var siteIds   = sites.Select(x => x.Id).ToList();
            var siteStats = await _insightsCore.Get<List<InsightsStats>>($"siteStatistics/?siteIds={siteIds.ToString()}");

            // Then combine into one
            return new InsightsStats
            { 
                OpenCount   = siteStats.Sum( x=> x.OpenCount),
                UrgentCount = siteStats.Sum( x=> x.UrgentCount),
                HighCount   = siteStats.Sum( x=> x.HighCount),
                MediumCount = siteStats.Sum( x=> x.MediumCount),
                LowCount    = siteStats.Sum( x=> x.LowCount)
            };
        }

        public async IAsyncEnumerable<InsightsStats> Get(IEnumerable<(Guid CustomerId, Guid PortfolioId)> ids)
        {
            foreach(var id in ids)
            {
                yield return await Get(id);
            }
        }
    }
}

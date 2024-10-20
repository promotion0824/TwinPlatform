using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.ServicesApi.InsightApi
{
    public class InsightStatisticsResponse
    {
        public InsightStatisticsResponse()
        {
            StatisticsByPriority = Enumerable.Empty<SiteInsightStatistics>().ToList();
            StatisticsByStatus = Enumerable.Empty<SiteInsightStatisticsByStatus>().ToList();
        }
        public List<SiteInsightStatistics> StatisticsByPriority { get; set; }
        public List<SiteInsightStatisticsByStatus> StatisticsByStatus { get; set; }
    }
}

using System;
using Willow.Platform.Statistics;

namespace PlatformPortalXL.Models
{
    public class SiteInsightStatisticsByStatus
    {
        /// <summary>
        /// Id = Site Id
        /// </summary>
        public Guid Id { get; set; }
        public int InProgressCount { get; set; }
        public int NewCount { get; set; }
        public int OpenCount { get; set; }
        public int IgnoredCount { get; set; }
        public int ResolvedCount { get; set; }


        public static InsightsStatsByStatus MapTo(SiteInsightStatisticsByStatus siteStatByStatus)
        {
	        if (siteStatByStatus == null)
		        return null;
	        return new InsightsStatsByStatus
	        {
		        InProgressCount = siteStatByStatus.InProgressCount,
		        NewCount = siteStatByStatus.NewCount,
                OpenCount = siteStatByStatus.OpenCount,
		        IgnoredCount = siteStatByStatus.IgnoredCount,
                ResolvedCount = siteStatByStatus.ResolvedCount
	        };
        }
	}
}

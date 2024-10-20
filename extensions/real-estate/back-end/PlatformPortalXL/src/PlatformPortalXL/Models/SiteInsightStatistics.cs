using System;
using Willow.Platform.Statistics;

namespace PlatformPortalXL.Models
{
    public class SiteInsightStatistics
    {
        /// <summary>
        /// Id = Site Id
        /// </summary>
        public Guid Id { get; set; }
        public int OpenCount { get; set; }
        public int UrgentCount { get; set; }
        public int HighCount { get; set; }
        public int MediumCount { get; set; }
        public int LowCount { get; set; }


        public static InsightsStats MapTo(SiteInsightStatistics siteStat)
        {
	        if (siteStat == null)
		        return null;
	        return new InsightsStats
	        {
		        HighCount = siteStat.HighCount,
		        MediumCount = siteStat.MediumCount,
		        LowCount = siteStat.LowCount,
		        UrgentCount = siteStat.UrgentCount,
		        OpenCount = siteStat.OpenCount
	        };
        }
	}
}

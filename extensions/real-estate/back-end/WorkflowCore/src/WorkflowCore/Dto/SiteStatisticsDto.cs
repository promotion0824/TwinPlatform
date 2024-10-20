using System;
using System.Linq;
using System.Collections.Generic;

namespace WorkflowCore.Dto
{
    public class SiteStatisticsDto
    {
        public Guid Id              { get; set; }
        public int  OverdueCount    { get; set; }
        public int  UrgentCount     { get; set; }
        public int  HighCount       { get; set; }
        public int  MediumCount     { get; set; }
        public int  LowCount        { get; set; }
        public int  OpenCount       { get; set; }

        public static SiteStatisticsDto MapFromModel(SiteStatistics siteStatistics)
        {
            return new SiteStatisticsDto
            {
                Id           =  siteStatistics.Id,
                OverdueCount = siteStatistics.OverdueCount,
                UrgentCount  = siteStatistics.UrgentCount,
                HighCount    = siteStatistics.HighCount,
                MediumCount  = siteStatistics.MediumCount,
                LowCount     = siteStatistics.LowCount,
                OpenCount    = siteStatistics.OpenCount
            };
        }

        public static List<SiteStatisticsDto> MapFromModels(List<SiteStatistics> siteStatisticsList)
        {
            return siteStatisticsList.Select(MapFromModel).ToList();
        }
    }

}
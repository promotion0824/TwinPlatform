using System;
using System.Linq;
using System.Collections.Generic;

namespace WorkflowCore.Dto
{
    public class InsightStatisticsDto
    {
        public Guid Id { get; set; }
        public int OverdueCount { get; set; }
        public int ScheduledCount { get; set; }
        public int TotalCount { get; set; }

        public static InsightStatisticsDto MapFromModel(InsightStatistics insightStatistics)
        {
			if (insightStatistics == null)
			{
				return null;
			}

			return new InsightStatisticsDto
            {
                Id = insightStatistics.Id,
                OverdueCount = insightStatistics.OverdueCount,
                ScheduledCount = insightStatistics.ScheduledCount,
                TotalCount = insightStatistics.TotalCount
            };
        }

        public static List<InsightStatisticsDto> MapFromModels(List<InsightStatistics> insightStatisticsList)
        {
            return insightStatisticsList.Select(MapFromModel).ToList();
        }
    }

}

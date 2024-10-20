using System.Collections.Generic;

namespace InsightCore.Dto
{
    public class InsightStatisticsResponse
    {
        public List<InsightStatisticsByPriority> StatisticsByPriority { get; set; }
        public List<InsightStatisticsByStatus> StatisticsByStatus { get; set; }
    }
}

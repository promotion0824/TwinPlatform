using System.Collections.Generic;

namespace WorkflowCore.Dto
{
    public class TicketStatisticsDto
    {
        public List<SiteStatistics> StatisticsByPriority { get; set; }
        public List<SiteTicketStatisticsByStatus> StatisticsByStatus { get; set; }
    }
}

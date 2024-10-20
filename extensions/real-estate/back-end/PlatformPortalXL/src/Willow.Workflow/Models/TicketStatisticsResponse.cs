using System.Collections.Generic;
using System.Linq;
using Willow.Platform.Models;

namespace Willow.Workflow.Models;
public class TicketStatisticsResponse
{
    public TicketStatisticsResponse()
    {
        StatisticsByPriority = Enumerable.Empty<TicketStatisticsByPriority>().ToList();
        StatisticsByStatus = Enumerable.Empty<TicketStatisticsByStatus>().ToList();
    }
    public List<TicketStatisticsByPriority> StatisticsByPriority { get; set; }
    public List<TicketStatisticsByStatus> StatisticsByStatus { get; set; }
}

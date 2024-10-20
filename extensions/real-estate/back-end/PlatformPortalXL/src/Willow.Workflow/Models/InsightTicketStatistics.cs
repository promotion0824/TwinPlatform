using System;

namespace Willow.Workflow
{
    public class InsightTicketStatistics
    {
        public Guid Id { get; set; }
        public int OverdueCount { get; set; }
        public int ScheduledCount { get; set; }
        public int TotalCount { get; set; }
    }
}

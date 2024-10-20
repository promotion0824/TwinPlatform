using System;

namespace Willow.Workflow
{
    public class SiteTicketStatistics
    {
        public Guid Id { get; set; }
        public int OverdueCount { get; set; }
        public int UrgentCount { get; set; }
        public int HighCount { get; set; }
        public int MediumCount { get; set; }
    }
}

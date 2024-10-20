using System;

namespace WorkflowCore.Dto
{
    public class SiteTicketStatisticsByStatus
    {
        public Guid Id { get; set; }
        public int OpenCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }
    }
}

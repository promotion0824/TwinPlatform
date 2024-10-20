using System;

namespace WorkflowCore.Dto
{
    public class TwinTicketStatisticsByStatus
    {
        public string TwinId { get; set; }
        public int OpenCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }
    }
}

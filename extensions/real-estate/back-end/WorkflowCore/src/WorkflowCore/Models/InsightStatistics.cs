using System;

namespace WorkflowCore.Dto
{
    public class InsightStatistics
    {
        public Guid Id { get; set; }
		public int OverdueCount { get; set; }
		public int ScheduledCount { get; set; }
		public int TotalCount { get; set; }
    }
}

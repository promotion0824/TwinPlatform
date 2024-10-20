using System;

namespace WorkflowCore.Models
{
    public class ZoneStatistics
    {
        public int CheckCount { get; set; }
        public DateTime? LastCheckSubmittedDate { get; set; }
        public int CompletedCheckCount { get; set; }
        public int WorkableCheckCount { get; set; }
        public CheckRecordStatus? WorkableCheckSummaryStatus { get; set; }
        public int InspectionCount { get; set; }
    }
}

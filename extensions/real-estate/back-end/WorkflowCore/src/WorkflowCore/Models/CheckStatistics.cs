using System;

namespace WorkflowCore.Models
{
    public class CheckStatistics
    { 
        public int CheckRecordCount { get; set; }
        public string LastCheckSubmittedEntry { get; set; }
        public Guid? LastCheckSubmittedUserId { get; set; }
        public DateTime? LastCheckSubmittedDate { get; set; }
        public CheckRecordStatus? WorkableCheckStatus { get; set; }
        public DateTime? NextCheckRecordDueTime { get; set; }
    }
}

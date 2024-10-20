using System;
using System.Collections.Generic;

namespace Willow.Workflow
{
    public class Inspection
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string Name { get; set; }
        public Guid ZoneId { get; set; }
        public string FloorCode { get; set; }
        [Obsolete("Use TwinId instead")]
        public Guid AssetId { get; set; }
        public string TwinId { get; set; }
        public Guid AssignedWorkgroupId { get; set; }
        public int Frequency { get; set; }
        public SchedulingUnit FrequencyUnit { get; set; }
        public List<DayOfWeek> FrequencyDaysOfWeek { get; set; }
        public  string NextEffectiveDate { get; set; }
        public  string StartDate { get; set; }
        public  string EndDate { get; set; }
        public int SortOrder { get; set; }
        public List<Check> Checks { get; set; }
        public int CheckRecordCount { get; set; }
        public int WorkableCheckCount { get; set; }
        public int CompletedCheckCount { get; set; }
        public DateTime? NextCheckRecordDueTime { get; set; }
        public CheckRecordStatus? CheckRecordSummaryStatus { get; set; }
    }
}

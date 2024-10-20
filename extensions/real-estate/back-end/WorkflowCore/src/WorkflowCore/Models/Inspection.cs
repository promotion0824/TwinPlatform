using System;
using System.Collections.Generic;

namespace WorkflowCore.Models
{
    public class Inspection
	{
		public Guid Id { get; set; }
		public Guid SiteId { get; set; }
		public string Name { get; set; }
		public string FloorCode { get; set; }
		public Guid ZoneId { get; set; }
		public Guid AssetId { get; set; }
		public string TwinId { get; set; }
		public Guid AssignedWorkgroupId { get; set; }
		public int Frequency { get; set; }
		public SchedulingUnit FrequencyUnit { get; set; }
        public IEnumerable<DayOfWeek> FrequencyDaysOfWeek { get; set; }
        public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public Guid? LastRecordId { get; set; }
		public bool IsArchived { get; set; }
		public int SortOrder { get; set; }
		public List<Check> Checks { get; set; } = new List<Check>();
        public InspectionRecord LastRecord { get; set; }
		public int CheckRecordCount { get; set; }
		public CheckRecordStatus? CheckRecordSummaryStatus { get; set; }
		public DateTime? NextCheckRecordDueTime { get; set; }
		public DateTime? LastCheckSubmittedDate { get; set; }
		public int WorkableCheckCount { get; set; }
		public int CompletedCheckCount { get; set; }
        public Zone Zone { get; set; }
}
}

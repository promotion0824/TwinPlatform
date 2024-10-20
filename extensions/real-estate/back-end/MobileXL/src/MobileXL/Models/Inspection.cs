using System;
using System.Collections.Generic;
using MobileXL.Models.Enums;

namespace MobileXL.Models
{
    public class Inspection
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FloorCode { get; set; }
        public Guid ZoneId { get; set; }
        public Guid AssetId { get; set; }
        public string TwinId { get; set; }
        public Guid AssignedWorkgroupId { get; set; }
		public int Frequency { get; set; }
		public SchedulingUnit FrequencyUnit { get; set; }
		public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int SortOrder { get; set; }

        public List<Check> Checks { get; set; }
        public InspectionRecord LastRecord { get; set; }

		public CheckRecordStatus? CheckRecordSummaryStatus { get; set; }
		public DateTime? NextCheckRecordDueTime { get; set; }
    }
}

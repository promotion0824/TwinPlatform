using System;

namespace WorkflowCore.Models
{
    public class Check
	{
		public Guid Id { get; set; }
		public Guid InspectionId { get; set; }
		public int SortOrder { get; set; }
		public string Name { get; set; }
		public CheckType Type { get; set; }
		public string TypeValue { get; set; }
		public int DecimalPlaces { get; set; }
		public double? MinValue { get; set; }
		public double? MaxValue { get; set; }
        public double Multiplier { get; set; } 
        public Guid? DependencyId { get; set; }
		public string DependencyValue { get; set; }
		public DateTime? PauseStartDate { get; set; }
		public DateTime? PauseEndDate { get; set; }
		public Guid? LastRecordId { get; set; }
		public Guid? LastSubmittedRecordId { get; set; }
		public bool IsArchived { get; set; }
		public bool CanGenerateInsight { get; set; }

		public CheckRecord LastRecord { get; set; }
        public CheckRecord LastSubmittedRecord { get; set; }
		public CheckStatistics Statistics { get; set; }
	}
}

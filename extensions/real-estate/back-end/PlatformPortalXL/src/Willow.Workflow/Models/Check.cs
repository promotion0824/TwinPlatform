using System;
using System.Collections.Generic;

namespace Willow.Workflow
{
    public class Check
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
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
        public bool IsArchived { get; set; }
        public bool CanGenerateInsight { get; set; }
        public List<CheckRecord> CheckRecords { get; set; }
        public CheckRecord LastSubmittedRecord { get; set; }
        public InspectionCheckStatistics  Statistics { get; set; }
    }
}

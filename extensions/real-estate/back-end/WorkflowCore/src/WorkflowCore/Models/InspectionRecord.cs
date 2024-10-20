using System;
using System.Collections.Generic;

namespace WorkflowCore.Models
{
    public class InspectionRecord
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public Guid InspectionId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int Occurrence { get; set; } = 0;

        public IList<CheckRecord> CheckRecords { get; set; } = new List<CheckRecord>();
    }
}

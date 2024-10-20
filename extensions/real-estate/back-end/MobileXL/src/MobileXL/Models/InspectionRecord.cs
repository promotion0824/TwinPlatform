using System;
using System.Collections.Generic;

namespace MobileXL.Models
{
    public class InspectionRecord
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
		public DateTime EffectiveAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public Inspection Inspection { get; set; }
        public List<CheckRecord> CheckRecords { get; set; }
    }
}

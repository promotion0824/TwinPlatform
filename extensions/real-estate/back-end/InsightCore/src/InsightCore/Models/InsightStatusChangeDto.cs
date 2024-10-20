using System;

namespace InsightCore.Models
{
    public class InsightStatusChangeDto
    {		
        public InsightStatus? Status { get; set; }
        public Guid? SourceId { get; set; }
		public Guid? UserId { get; set; }
		public string Reason { get; set; }
	}
}

using System;
using System.Collections.Generic;


namespace WorkflowCore.Models
{
    public class CheckRecord
	{
		public Guid Id { get; set; }
		public Guid InspectionId { get; set; }
		public Guid CheckId { get; set; }
		public Guid InspectionRecordId { get; set; }
		public CheckRecordStatus Status { get; set; }
		public Guid? SubmittedUserId { get; set; }
		public DateTime? SubmittedDate { get; set; }
		public DateTime? SubmittedSiteLocalDate { get; set; }
        public DateTime? SyncedDate { get; set; }
        public DateTime? SyncedSiteLocalDate { get; set; }
        public double? NumberValue { get; set; }
        public string TypeValue { get; set; }
        public string StringValue { get; set; }
		public DateTime? DateValue { get; set; }
		public string Notes { get; set; }
		public Guid? InsightId { get; set; }
		public DateTime EffectiveDate { get; set; }
		public List<AttachmentBase> Attachments { get; set; }
	}
}

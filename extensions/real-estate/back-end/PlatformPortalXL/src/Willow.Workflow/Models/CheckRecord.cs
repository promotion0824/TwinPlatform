using System;
using System.Collections.Generic;

namespace Willow.Workflow
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
        public double? NumberValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string StringValue { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Notes { get; set; }
        public List<Attachment> Attachments { get; set; }
    }
}

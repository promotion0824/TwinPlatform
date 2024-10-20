using System;
using System.Collections.Generic;

namespace Willow.Workflow
{
    public class CheckRecordReport
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
        public string InspectionName { get; set; }
        public Guid CheckId { get; set; }
        public string CheckName { get; set; }
        public Guid ZoneId { get; set; }
        public string ZoneName { get; set; }
        public string FloorCode { get; set; }
        public string TwinName { get; set; }
        public Guid InspectionRecordId { get; set; }
        public CheckRecordStatus Status { get; set; }
        public Guid? SubmittedUserId { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? SubmittedSiteLocalDate { get; set; }
        public double? NumberValue { get; set; }
        public string StringValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string Notes { get; set; }
        public Guid? InsightId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public CheckType CheckType { get; set; }
        public string TypeValue { get; set; }
        public List<Attachment> Attachments { get; set; }
    }
}

using System;

namespace WorkflowCore.Controllers.Request
{
    public class GenerateInspectionRecordRequest
    {
        public Guid     InspectionId  { get; set; }
        public Guid     SiteId        { get; set; }
        public DateTime HitTime       { get; set; }
        public DateTime SiteNow       { get; set; }
    }

    public class GenerateCheckRecordRequest
    {
        public Guid     InspectionId        { get; set; }
        public Guid     InspectionRecordId  { get; set; }
        public Guid     CheckId             { get; set; }
        public Guid     SiteId              { get; set; }
        public DateTime EffectiveDate       { get; set; }
    }
}
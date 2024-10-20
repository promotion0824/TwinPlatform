using System;
using System.Collections.Generic;
using WorkflowCore.Models;

namespace WorkflowCore.Controllers.Request
{
    public class SubmitCheckRecordRequest
    {
        public double? NumberValue { get; set; }
        public string StringValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string Notes { get; set; }
        public Guid SubmittedUserId { get; set; }
        public string SubmittedUserFullname { get; set; }
		public string TimeZoneId { get; set; }
        public List<AttachmentBase> Attachments { get; set; }
        public DateTime? EnteredAt { get; set; }
    }
}

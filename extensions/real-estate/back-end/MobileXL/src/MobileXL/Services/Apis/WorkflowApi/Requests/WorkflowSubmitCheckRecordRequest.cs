using MobileXL.Models;
using System;
using System.Collections.Generic;

namespace MobileXL.Services.Apis.WorkflowApi.Requests
{
    public class WorkflowSubmitCheckRecordRequest
    {
        public double? NumberValue { get; set; }
        public string StringValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string Notes { get; set; }
        public Guid SubmittedUserId { get; set; }
        public string SubmittedUserFullname { get; set; }
		public string TimeZoneId { get; set; }
        public List<Attachment> Attachments { get; set; }
        public DateTime? EnteredAt { get; set; }
    }
}

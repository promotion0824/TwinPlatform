using MobileXL.Models;
using System;
using System.Collections.Generic;

namespace MobileXL.Features.Inspections.Requests
{
    public class SubmitCheckRecordRequest
    {
        public double? NumberValue { get; set; }
        public string StringValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string Notes { get; set; }
        public List<Attachment> Attachments { get; set; }
    }
}

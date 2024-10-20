using MobileXL.Dto;
using System;

namespace MobileXL.Features.Inspections.Requests
{
    public class CheckRecordRequest
    {
        public Guid Id { get; set; }
        public double? NumberValue { get; set; }
        public string StringValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string Notes { get; set; }
        public DateTime? EnteredAt { get; set; }
    }
}

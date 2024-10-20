using System;
using System.Collections.Generic;

namespace MobileXL.Features.Inspections.Requests
{
    public class InspectionRecordRequest
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
        public List<CheckRecordRequest> CheckRecords { get; set; }
    }
}

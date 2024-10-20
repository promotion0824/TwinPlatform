using MobileXL.Features.Inspections.Requests;
using System.Collections.Generic;
using System;

namespace MobileXL.Features.Inspections.Response
{
    public class InspectionRecordResponse
    {
        public Guid Id { get; set; }
        public List<CheckRecordResponse> CheckRecords { get; set; }
    }
}

using MobileXL.Dto;
using System;

namespace MobileXL.Features.Inspections.Response
{
    public class CheckRecordResponse
    {
        public Guid Id { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
    }
}

using PlatformPortalXL.Models;
using System;

namespace PlatformPortalXL.Dto
{
    public class AvailableSetPointCommandDto
    {
        public Guid InsightId { get; set; }
        public Guid PointId { get; set; }
        public Guid SetPointId { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal OriginalValue { get; set; }
        public string Unit { get; set; }
        public SetPointCommandType Type { get; set; }        
        public decimal DesiredValueLimitation { get; set; }
    }
}

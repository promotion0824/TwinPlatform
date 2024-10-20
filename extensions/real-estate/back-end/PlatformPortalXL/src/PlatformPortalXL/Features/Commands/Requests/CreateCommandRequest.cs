using PlatformPortalXL.Models;
using System;

namespace PlatformPortalXL.Features.Commands.Requests
{
    public class CreateCommandRequest
    {
        public Guid InsightId { get; set; }
        public Guid PointId { get; set; }
        public Guid SetPointId { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal OriginalValue { get; set; }
        public decimal DesiredValue { get; set; }
        public int DesiredDurationMinutes { get; set; }
        public SetPointCommandType Type { get; set; }
    }
}

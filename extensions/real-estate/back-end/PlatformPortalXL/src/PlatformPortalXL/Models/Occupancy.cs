using System;

namespace PlatformPortalXL.Models
{
    public class Occupancy
    {
        public Guid FloorId { get; set; }
        public string FloorName { get; set; }
        public string FloorCode { get; set; }
        public int? RunningTotal { get; set; }
        public int? FloorLimit { get; set; }
        public Guid ReportId { get; set; }
    }
}

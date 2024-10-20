using System;

namespace SiteCore.Domain
{
    // Copy temp soultion from PortalXL
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

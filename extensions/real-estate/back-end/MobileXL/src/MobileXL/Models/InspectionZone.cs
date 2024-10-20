using System;

namespace MobileXL.Models
{
    public class InspectionZone
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public InspectionZoneStatistics Statistics { get; set; }
    }
}

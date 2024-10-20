using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;

namespace MobileXL.Dto
{
    public class InspectionZoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public InspectionZoneStatistics Statistics { get; set; }

        public static InspectionZoneDto Map(InspectionZone zone)
        {
            if (zone == null)
            {
                return null;
            }

            return new InspectionZoneDto
            {
                Id = zone.Id,
                Name = zone.Name,
                Statistics = zone.Statistics
            };
        }

        public static List<InspectionZoneDto> Map(List<InspectionZone> zones)
        {
            return zones?.Select(Map).ToList();
        }
    }
}

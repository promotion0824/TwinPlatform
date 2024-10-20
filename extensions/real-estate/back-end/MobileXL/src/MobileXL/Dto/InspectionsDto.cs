using MobileXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileXL.Dto
{
    public class InspectionsDto
    {
        public List<SiteDto> Sites { get; set; }
    }

    public class SiteDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<ZoneDto> InspectionZones { get; set; }

        public static SiteDto Map(Site site)
        {
            if (site == null)
            {
                return null;
            }

            return new SiteDto
            {
                Id = site.Id,
                Name = site.Name
            };
        }

        public static List<SiteDto> Map(List<Site> sites)
        {
            return sites?.Select(Map).ToList();
        }
    }

    public class ZoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<InspectionDto> Inspections { get; set; }

        public static ZoneDto Map(InspectionZone zone)
        {
            if (zone == null)
            {
                return null;
            }

            return new ZoneDto
            {
                Id = zone.Id,
                Name = zone.Name
            };
        }

        public static List<ZoneDto> Map(List<InspectionZone> zones)
        {
            return zones?.Select(Map).ToList();
        }
    }
}

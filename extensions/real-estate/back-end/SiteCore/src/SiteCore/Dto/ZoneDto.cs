using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SiteCore.Domain;

namespace SiteCore.Dto
{
    public class ZoneDto
    {
        public Guid Id { get; set; }
        public List<List<int>> Geometry { get; set; } = new List<List<int>>();
        public List<Guid> EquipmentIds { get; set; } = new List<Guid>();

        public static ZoneDto MapFrom(Zone zone)
        {
            if (zone == null)
            {
                return null;
            }

            return new ZoneDto
            {
                Id = zone.Id,
                Geometry = string.IsNullOrEmpty(zone.Geometry) ? new List<List<int>>() : JsonConvert.DeserializeObject<List<List<int>>>(zone.Geometry)
            };
        }
    }
}

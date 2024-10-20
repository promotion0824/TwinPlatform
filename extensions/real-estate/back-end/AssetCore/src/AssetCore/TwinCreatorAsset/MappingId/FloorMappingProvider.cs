using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AssetCoreTwinCreator.MappingId.Models;

namespace AssetCoreTwinCreator.MappingId
{
    public class FloorMappingProvider
    {
        private List<FloorMappingInternal> _internalMappings;

        public void Initialize(IEnumerable<FloorMapping> floorMappings, IEnumerable<SiteMapping> siteMappings)
        {
            _internalMappings = (from floorMapping in floorMappings
                    from siteMapping in siteMappings
                    where floorMapping.BuildingId == siteMapping.BuildingId
                    select new FloorMappingInternal
                    {
                        FloorCode = floorMapping.FloorCode,
                        FloorId = floorMapping.FloorId,
                        BuildingId = siteMapping.BuildingId,
                        SiteId = siteMapping.SiteId
                    })
                .ToList();
        }

        public string GetFloorCode(Guid floorId)
        {
            var floorCode = _internalMappings.FirstOrDefault(m => m.FloorId == floorId)?.FloorCode;

            if (string.IsNullOrEmpty(floorCode))
            {
                throw new InvalidDataException($"Mapping for floor id {floorId} is not defined"); 
            }

            return floorCode;
        }

        public Guid GetFloorId(string floorCode, int buildingId)
        {
            var floorGuid = _internalMappings.FirstOrDefault(m => m.BuildingId == buildingId && m.FloorCode.Equals(floorCode, StringComparison.InvariantCultureIgnoreCase))?.FloorId;

            if (!floorGuid.HasValue)
            {
                return Guid.Empty;
            }

            return floorGuid.Value;
        }


        private class FloorMappingInternal
        {
            public Guid FloorId { get; set; }

            public int BuildingId { get; set; }

            public string FloorCode { get; set; }

            public Guid SiteId { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class FloorDetailDto
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int SortOrder { get; set; }
        public string Geometry { get; set; }
        public Guid? ModelReference { get; set; }

        public static FloorDetailDto MapFrom(Floor floor)
        {
            if (floor == null)
            {
                return null;
            }

            return new FloorDetailDto
            {
                Id = floor.Id,
                SiteId = floor.SiteId,
                Name = floor.Name,
                Code = floor.Code,
                SortOrder = floor.SortOrder,
                Geometry = floor.Geometry,
                ModelReference = floor.ModelReference
            };
        }

        public static IEnumerable<FloorDetailDto> MapFrom(IEnumerable<Floor> floors)
        {
            return floors?.Select(MapFrom).ToList();
        }
    }
}

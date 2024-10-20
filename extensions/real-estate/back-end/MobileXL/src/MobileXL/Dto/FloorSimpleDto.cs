using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;

namespace MobileXL.Dto
{
    public class FloorSimpleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Geometry { get; set; }

        public static FloorSimpleDto MapFrom(Floor floor)
        {
            if (floor == null)
            {
                return null;
            }

            return new FloorSimpleDto
            {
                Id = floor.Id,
                Name = floor.Name,
                Code = floor.Code,
                Geometry = floor.Geometry
            };
        }

        public static List<FloorSimpleDto> MapFrom(IEnumerable<Floor> floors)
        {
            return floors?.Select(MapFrom).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class FloorSimpleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Geometry { get; set; }
        public bool IsSiteWide { get; set; }        
        public Guid? ModelReference { get; set; }
		[Obsolete("This field is no longer provide a valid value")]
        public int? InsightsMaxPriority => this.InsightsHighestPriority;
        [Obsolete("This field is no longer provide a valid value")]
		public int? InsightsHighestPriority { get; set; }

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
                Geometry = floor.Geometry,
                IsSiteWide = floor.IsSiteWide,
                ModelReference = floor.ModelReference
            };
        }

        public static List<FloorSimpleDto> MapFrom(IEnumerable<Floor> floors)
        {
            return floors?.Select(MapFrom).ToList();
        }
    }
}

using DigitalTwinCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Dto
{
    public class TwinWithRelationshipsDto
    {
        public TwinDto Twin { get; set; }
        public List<RelationshipDto> Relationships { get; set; }

        internal static TwinWithRelationshipsDto MapFrom(TwinWithRelationships twin)
        {
            return new TwinWithRelationshipsDto { 
                Twin = TwinDto.MapFrom(twin),
                Relationships = RelationshipDto.MapFrom(twin.Relationships)
            };
        }

        internal static List<TwinWithRelationshipsDto> MapFrom(IEnumerable<TwinWithRelationships> twins)
        {
            if (twins == null)
            {
                return new List<TwinWithRelationshipsDto>();
            }
            return twins.Select(MapFrom).ToList();
        }
    }
}

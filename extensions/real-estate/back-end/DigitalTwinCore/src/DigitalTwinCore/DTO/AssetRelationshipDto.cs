using DigitalTwinCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Dto
{
    public class AssetRelationshipDto
    {
        public RelationshipDto Relationship { get; set; }
        public TwinDto Target { get; set; }

        public static AssetRelationshipDto MapFrom(TwinRelationship relationship)
        {
            return new AssetRelationshipDto
            {
                Relationship = RelationshipDto.MapFrom(relationship),
                Target = TwinDto.MapFrom(relationship.Target)
            };
        }

        public static List<AssetRelationshipDto> MapFrom(IEnumerable<TwinRelationship> relationships)
        {
            return relationships?.Select(MapFrom).ToList() ?? new List<AssetRelationshipDto>();
        }
    }

    public class AssetIncomingRelationshipDto
    {
        public IncomingRelationshipDto Relationship { get; set; }
        public TwinDto Source { get; set; }

        public static AssetIncomingRelationshipDto MapFrom(TwinRelationship relationship)
        {
            return new AssetIncomingRelationshipDto
            {
                Relationship = IncomingRelationshipDto.MapFrom(relationship),
                Source = TwinDto.MapFrom(relationship.Source)
            };
        }

        public static List<AssetIncomingRelationshipDto> MapFrom(List<TwinRelationship> incomingRelationships)
        {
            return incomingRelationships?.Select(MapFrom).ToList() ?? new List<AssetIncomingRelationshipDto>();
        }
    }
}

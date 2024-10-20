using Azure.DigitalTwins.Core;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.DTO;

public class RelationshipMapRelationshipDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Direction Direction { get; set; }
    public RelationshipMapTwinDto TargetTwin { get; set; }

    public static RelationshipMapRelationshipDto MapFrom(BasicRelationship relationship)
    {
        if (relationship == null)
        {
            return null;
        }

        return new RelationshipMapRelationshipDto
        {
            Id = relationship.Id,
            Name = relationship.Name,
            Direction = Direction.Out,
            TargetTwin = new RelationshipMapTwinDto { Id = relationship.TargetId }
        };
    }

    public static List<RelationshipMapRelationshipDto> MapFrom(List<BasicRelationship> relationships)
    {
        return relationships?.Select(MapFrom).ToList() ?? new List<RelationshipMapRelationshipDto>();
    }
    public static RelationshipMapRelationshipDto MapFrom(IncomingRelationship incomingRelationship)
    {
        if (incomingRelationship == null)
        {
            return null;
        }

        return new RelationshipMapRelationshipDto
        {
            Id = incomingRelationship.RelationshipId,
            Name = incomingRelationship.RelationshipName,
            Direction = Direction.In,
            TargetTwin = new RelationshipMapTwinDto { Id = incomingRelationship.SourceId }
        };
    }
    public static List<RelationshipMapRelationshipDto> MapFrom(List<IncomingRelationship> incomingRelationships)
    {
        return incomingRelationships?.Select(MapFrom).ToList() ?? new List<RelationshipMapRelationshipDto>();
    }
}

public enum Direction
{
    In,
    Out
}
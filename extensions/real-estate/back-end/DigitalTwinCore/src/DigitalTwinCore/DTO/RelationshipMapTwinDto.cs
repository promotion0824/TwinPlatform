using System.Collections.Generic;

namespace DigitalTwinCore.DTO;

public class RelationshipMapTwinDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<RelationshipMapRelationshipDto> Relationships { get; set; }
}

using System.Collections.Generic;

namespace DigitalTwinCore.Dto
{
    public class AssetRelationshipsDto
    {
        public List<AssetRelationshipDto> Relationships { get; set; }
        public List<AssetIncomingRelationshipDto> IncomingRelationships { get; set; }
    }
}

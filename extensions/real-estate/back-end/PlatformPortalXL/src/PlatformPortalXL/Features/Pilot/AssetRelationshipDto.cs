
namespace PlatformPortalXL.Features.Pilot
{
    public class AssetRelationshipDto
    {
        public RelationshipDto Relationship { get; set; }
        public TwinDto Target { get; set; }
    }

    public class AssetIncomingRelationshipDto
    {
        public IncomingRelationshipDto Relationship { get; set; }
        public TwinDto Source { get; set; }
    }
}

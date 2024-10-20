using Azure.DigitalTwins.Core;

namespace DigitalTwinCore.Models
{
    public class TwinIncomingRelationship
    {
        public string RelationshipId { get; set; }
        public string SourceId { get; set; }
        public string RelationshipName { get; set; }
        public string RelationshipLink { get; set; }

        public static TwinIncomingRelationship MapFrom(IncomingRelationship incomingRelationship)
        {
            if (incomingRelationship == null)
                return null;

            return new TwinIncomingRelationship
            {
                RelationshipId = incomingRelationship.RelationshipId,
                RelationshipLink = incomingRelationship.RelationshipLink,
                RelationshipName = incomingRelationship.RelationshipName,
                SourceId = incomingRelationship.SourceId
            };
        }
    }
}

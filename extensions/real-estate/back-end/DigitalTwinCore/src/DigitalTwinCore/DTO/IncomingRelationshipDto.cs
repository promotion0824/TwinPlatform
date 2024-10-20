using DigitalTwinCore.Models;

namespace DigitalTwinCore.Dto
{
    public class IncomingRelationshipDto
    {
        public string RelationshipId { get; set; }
        public string SourceId { get; set; }
        public string RelationshipName { get; set; }
        public string RelationshipLink { get; set; }

        internal static IncomingRelationshipDto MapFrom(TwinRelationship model)
        {
            return new IncomingRelationshipDto
            {
                RelationshipId = model.Id,
                SourceId = model.Source.Id,
                RelationshipName = model.Name,
                RelationshipLink = model.Source.Metadata.ModelId
            };
        }

        internal static IncomingRelationshipDto MapFrom(TwinIncomingRelationship model)
        {
            return new IncomingRelationshipDto
            {
                RelationshipId = model.RelationshipId,
                SourceId = model.SourceId,
                RelationshipName = model.RelationshipName,
                RelationshipLink = model.RelationshipLink
            };
        }
    }
}

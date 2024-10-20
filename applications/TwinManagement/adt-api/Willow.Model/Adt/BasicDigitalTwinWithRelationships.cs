using Azure.DigitalTwins.Core;

namespace Willow.Model.Adt
{
    public class BasicDigitalTwinWithRelationships
    {
        public BasicDigitalTwin Twin { get; set; }
        public List<BasicDigitalTwin> Twins { get; set; }
        public List<BasicRelationship> Relationships { get; set; }
        public BasicDigitalTwinWithRelationships(BasicDigitalTwin twin, List<BasicDigitalTwin>? twins = null, List<BasicRelationship>? relationships = null)
        {
            Twin = twin;
            Twins = twins ?? new List<BasicDigitalTwin>();
            Relationships = relationships ?? new List<BasicRelationship>();
        }
    }
}

namespace DigitalTwinCore.Features.RelationshipMap.Models
{
    public struct Edge
    {
        public string Id { get; set; }

        public string RelationshipType { get; set; }

        public string Substance { get; set; }

        public BasicDigitalTwin Destination { get; set; }
    }
}

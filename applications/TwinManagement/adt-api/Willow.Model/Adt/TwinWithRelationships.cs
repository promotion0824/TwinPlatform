using Azure.DigitalTwins.Core;

namespace Willow.Model.Adt;

public class TwinWithRelationships
{
    public BasicDigitalTwin Twin { get; set; } = new BasicDigitalTwin();

    public List<BasicRelationship> IncomingRelationships { get; set; } = new List<BasicRelationship>();

    public List<BasicRelationship> OutgoingRelationships { get; set; } = new List<BasicRelationship>();

    public Dictionary<string, object>? TwinData { get; set; }
}

namespace Willow.Model.Requests;

public class BulkDeleteRelationshipsRequest
{
    public BulkDeleteRelationshipsRequest()
    {
        TwinIds = new List<string>();
        RelationshipIds = new List<string>();
    }

    public bool DeleteAll { get; set; }
    public IEnumerable<string> TwinIds { get; set; }
    public IEnumerable<string> RelationshipIds { get; set; }
}

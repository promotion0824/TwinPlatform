namespace Willow.Model.Requests;

public class BulkDeleteModelsRequest
{
    public BulkDeleteModelsRequest()
    {
        ModelIds = new List<string>();
    }

    public bool DeleteAll { get; set; }
    public IEnumerable<string> ModelIds { get; set; }
    public bool IncludeDependencies { get; set; }
}

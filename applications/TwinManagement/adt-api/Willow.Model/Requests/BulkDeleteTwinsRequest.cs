namespace Willow.Model.Requests;

public class BulkDeleteTwinsRequest
{
    public BulkDeleteTwinsRequest()
    {
        TwinIds = new List<string>();
        ModelIds = Enumerable.Empty<string>();
        LocationId = string.Empty;
        SearchString = string.Empty;
        Filters = new Dictionary<string, string>();
    }

    public bool DeleteAll { get; set; }
    public IEnumerable<string> TwinIds { get; set; }
    public IEnumerable<string> ModelIds { get; set; }
    public string LocationId { get; set; }
    public string SearchString { get; set; }
    public Dictionary<string, string> Filters { get; set; }
}

using Newtonsoft.Json;
using System.Data;

namespace Willow.AzureDataExplorer.Query;

public class PageQueryResult
{
    public string? QueryId { get; set; }

    public int NextPage { get; set; }

    public int Total { get; set; }

    [JsonIgnore]
    public IDataReader? ResultsReader { get; set; }
}

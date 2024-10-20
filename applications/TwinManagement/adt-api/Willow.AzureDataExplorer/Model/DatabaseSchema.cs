namespace Willow.AzureDataExplorer.Model;

public class DatabaseSchema
{
    public string? Name { get; set; }

    public IDictionary<string, TableSchema> Tables { get; set; } = new Dictionary<string, TableSchema>();
}

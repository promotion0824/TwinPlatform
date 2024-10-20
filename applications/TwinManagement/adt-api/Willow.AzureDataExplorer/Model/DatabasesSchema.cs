namespace Willow.AzureDataExplorer.Model;

sealed internal class DatabasesSchema
{
    public IDictionary<string, DatabaseSchema> Databases { get; set; } = new Dictionary<string, DatabaseSchema>();
}

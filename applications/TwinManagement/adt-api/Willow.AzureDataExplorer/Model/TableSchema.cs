namespace Willow.AzureDataExplorer.Model;

public class TableSchema
{
    public string? Name { get; set; }

    public IEnumerable<ColumnSchema> OrderedColumns { get; set; } = Array.Empty<ColumnSchema>();
}

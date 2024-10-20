namespace Willow.AzureDataExplorer.Options;

public class AzureDataExplorerOptions
{
    public string? ClusterName { get; set; }
    public string? ClusterRegion { get; set; }
    public string? ClusterUri { get; set; }
    public string? DatabaseName { get; set; }
    public AzureDataExplorerSchemaOptions? Schema { get; set; }
}


public class AzureDataExplorerSchemaOptions
{
    public string? DefaultSchemaName { get; set; }
    public bool EnableMigration { get; set; }

    public bool AllowBackfill { get; set; }
}

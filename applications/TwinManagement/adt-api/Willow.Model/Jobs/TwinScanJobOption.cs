namespace Willow.Model.Jobs;

/// <summary>
/// Twin Scan Job Option.
/// </summary>
public record TwinScanJobOption : JobBaseOption
{
    /// <summary>
    /// Job Query
    /// </summary>
    public string Query { get; set; } = default!;

    /// <summary>
    /// Number of twins to retrieve in each scan.
    /// </summary>
	public int QueryPageSize { get; set; }

    public Dictionary<string, string> ModelMigrations { get; set; } = [];
}

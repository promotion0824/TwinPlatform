namespace Willow.Model.Jobs;

/// <summary>
/// Twin Incremental Scan Job Option.
/// </summary>
public record TwinIncrementalScanJobOption : JobBaseOption
{
    /// <summary>
    /// Controls scope of twin last updated time for each scan
    /// </summary>
    public TimeSpan QueryBuffer { get; set; }

    /// <summary>
    /// Number of twins to retrieve in each scan.
    /// </summary>
	public int QueryPageSize { get; set; }

    /// <summary>
    /// Custom Data
    /// </summary>
    public TwinIncrementalScanJobCustomData? CustomData { get; set; }
}


/// <summary>
/// Twin Incremental Scan Job Option.
/// </summary>
public record TwinIncrementalScanJobCustomData
{
    /// <summary>
    /// Twin Last Updated From
    /// </summary>
    public DateTime UpdatedFrom { get; set; }

    /// <summary>
    /// Twin Last Updated From
    /// </summary>
    public DateTime UpdatedUntil { get; set; }
}

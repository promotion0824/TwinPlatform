using Willow.Batch;

namespace Willow.Model.Mapping;

public class MappedEntryRequest 
{
    /// <summary>
    /// Used to identify the starting point to return rows.
    /// </summary>
    public int offset { get; set; } = 0;

    /// <summary>
    /// Amount of records to fetch for each requests.
    /// </summary>
    public int pageSize { get; set; }  = 100;

    /// <summary>
    /// Prefixes to match with the first few characters of Mapped Id.
    /// </summary>
    public string[]? prefixToMatchId { get; set; } = null;

    /// <summary>
    /// Exclude records where prefixes match with the first few characters of Mapped Id.
    /// </summary>
    public bool? excludePrefixes { get; set; } = false;

    /// <summary>
    /// Gets or sets the filter specifications from MUI data grid.
    /// </summary>
    public FilterSpecificationDto[] FilterSpecifications { get; set; } = [];
}

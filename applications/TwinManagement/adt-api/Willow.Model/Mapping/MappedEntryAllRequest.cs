namespace Willow.Model.Mapping;

public class MappedEntryAllRequest 
{
    /// <summary>
    /// Prefixes to match with the first few characters of Mapped Id.
    /// </summary>
    public string[]? prefixToMatchId { get; set; } = null;

    /// <summary>
    /// Exclude records where prefixes match with the first few characters of Mapped Id.
    /// </summary>
    public bool? excludePrefixes { get; set; } = false;

    /// <summary>
    /// List of statuses to filter the records.
    /// </summary>
    public List<Status>? statuses { get; set; } = new List<Status>();

    /// <summary>
    /// List of building ids to filter the records.
    /// </summary>
    public string[]? buildingIds { get; set; } = Array.Empty<string>();

    /// <summary>
    /// connectorId to filter the records.
    /// </summary>
    public string? connectorId { get; set; } = string.Empty;
}

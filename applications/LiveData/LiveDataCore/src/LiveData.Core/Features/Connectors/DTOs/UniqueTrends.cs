namespace Willow.LiveData.Core.Features.Connectors.DTOs;

/// <summary>
/// Unique Trends.
/// </summary>
public class UniqueTrends
{
    /// <summary>
    /// Gets or sets the connector identifier.
    /// </summary>
    public string ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the count of total capabilities.
    /// </summary>
    public int TotalCapabilities { get; set; }

    /// <summary>
    /// Gets or sets the count of active capabilities.
    /// </summary>
    public int ActiveCapabilities { get; set; }

    /// <summary>
    /// Gets or sets the count of inactive capabilities.
    /// </summary>
    public int InactiveCapabilities { get; set; }

    /// <summary>
    /// Gets or sets the count of trending capabilities.
    /// </summary>
    public int TrendingCapabilities { get; set; }
}

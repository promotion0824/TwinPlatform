namespace Willow.CommandAndControl.Application.Requests.ActivityLogs.GetStatistics;

/// <summary>
/// Get Statistics Request DTO.
/// </summary>
public class GetStatisticsRequestDto
{
    /// <summary>
    /// Gets or sets the site ID for search.
    /// </summary>
    public string? SiteId { get; set; }

    /// <summary>
    /// Gets or sets the start date for the search.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date for the search.
    /// </summary>
    public DateTime? EndDate { get; set; }
}

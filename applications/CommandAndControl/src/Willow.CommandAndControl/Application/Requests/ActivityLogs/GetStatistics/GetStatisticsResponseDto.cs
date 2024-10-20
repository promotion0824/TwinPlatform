namespace Willow.CommandAndControl.Application.Requests.ActivityLogs.GetStatistics;

/// <summary>
/// Statistics for commands.
/// </summary>
public record GetStatisticsResponseDto
{
    /// <summary>
    /// Gets or sets the statistics count.
    /// </summary>
    public required CommandsCountStatisticsDto CommandsCount { get; set; }

    /// <summary>
    /// Gets or sets the list of most Recent Activities.
    /// </summary>
    public required List<ActivityLogsResponseDto> RecentActivities { get; set; } = new();

    /// <summary>
    /// Gets or sets the command Trends data to populate the chart.
    /// </summary>
    public required CommandTrendsDto CommandTrends { get; set; }
}

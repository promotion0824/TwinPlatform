namespace Willow.CommandAndControl.Application.Requests.ActivityLogs.GetStatistics;

/// <summary>
/// Command Trends DTO.
/// </summary>
public class CommandTrendsDto
{
    /// <summary>
    /// Gets or sets the categories for the chart.
    /// </summary>
    public required IList<string> Categories { get; set; }

    /// <summary>
    /// Gets or sets the data sets for the chart.
    /// </summary>
    public required IDictionary<string, CommandsTrendDataSetDto> Dataset { get; set; }
}

/// <summary>
/// Command Trend Data Set.
/// </summary>
public class CommandsTrendDataSetDto
{
    /// <summary>
    /// Gets or sets the name of the data set.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the list of data.
    /// </summary>
    public required IList<int> Data { get; set; }
}

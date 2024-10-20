namespace Willow.CommandAndControl.Application.Requests.ActivityLogs.GetStatistics;

/// <summary>
/// Statistics counts for commands.
/// </summary>
public class CommandsCountStatisticsDto
{
    /// <summary>
    /// Gets the total number of requested commands received.
    /// </summary>
    public required int TotalRequestedCommands { get; init; }

    /// <summary>
    /// Gets the total number of approved commands.
    /// </summary>
    public required int TotalApprovedCommands { get; init; }

    /// <summary>
    /// Gets the total number of resolved commands that have been executed.
    /// </summary>
    public required int TotalExecutedCommands { get; init; }

    /// <summary>
    /// Gets the total number of resolved commands that have been cancelled.
    /// </summary>
    public required int TotalCancelledCommands { get; init; }

    /// <summary>
    /// Gets the total number of currently suspended commands.
    /// </summary>
    public required int TotalSuspendedCommands { get; init; }

    /// <summary>
    /// Gets ttotal number of failed commands.
    /// </summary>
    public required int TotalFailedCommands { get; init; }
}

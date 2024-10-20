namespace Willow.CommandAndControl.Application.Services.Abstractions;

/// <summary>
/// Logs user activity.
/// </summary>
public interface IActivityLogger
{
    /// <summary>
    /// Creates an activity log entry.
    /// </summary>
    /// <param name="type">The type of activity.</param>
    /// <param name="command">The requested command.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task.</returns>
    ValueTask LogAsync(ActivityType type, RequestedCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an activity log entry.
    /// </summary>
    /// <param name="type">The type of activity.</param>
    /// <param name="command">The resolved command.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task.</returns>
    ValueTask LogAsync(ActivityType type, ResolvedCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an activity log entry.
    /// </summary>
    /// <param name="type">The type of activity.</param>
    /// <param name="command">The resolved command.</param>
    /// <param name="extraInfo">Extra Information to log.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task.</returns>
    ValueTask LogAsync(ActivityType type, ResolvedCommand command, string? extraInfo, CancellationToken cancellationToken = default);
}

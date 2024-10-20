namespace Willow.CommandAndControl.Data.Enums;

/// <summary>
/// Resolved Command Status.
/// </summary>
public enum ResolvedCommandStatus
{
    /// <summary>
    /// The command has been approved but not yet scheduled.
    /// </summary>
    /// <remarks>
    /// This supports the initial scenario where resolved commands must be.
    /// explicitly executed.
    /// </remarks>
    Approved = -1,

    /// <summary>
    /// Scheduled Command.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Command which suspended by user.
    /// </summary>
    Suspended = 1,

    /// <summary>
    /// Command which failed while executing.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Command which is executing and is not completed yet.
    /// </summary>
    Executing = 3,

    /// <summary>
    /// Command which cancelled by user.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Command which is executed successfully.
    /// </summary>
    Executed = 5,
}

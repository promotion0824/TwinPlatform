namespace Willow.CommandAndControl.Data.Enums;

/// <summary>
/// Type of user command.
/// </summary>
public enum ActivityType
{
    /// <summary>
    /// Indicates that the command has been received.
    /// </summary>
    Received = 1,

    /// <summary>
    /// Indicates that the command has been approved.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Indicates that the command has been rejected.
    /// </summary>
    Retracted = 3,

    /// <summary>
    /// Indicates that the command has been sent for execution.
    /// </summary>
    Executed = 4,

    /// <summary>
    /// Indicates that the command has failed.
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Indicates that the command has been cancelled.
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Indicates that the command has been suspended.
    /// </summary>
    Suspended = 7,

    /// <summary>
    /// Indicates the write message was sent to the controller.
    /// </summary>
    MessageSent = 8,

    /// <summary>
    /// Indicates that a success message was received from the controller.
    /// </summary>
    MessageReceivedSuccess = 9,

    /// <summary>
    /// Indicates that a failed message was received from the controller.
    /// </summary>
    MessageReceivedFailed = 10,

    /// <summary>
    /// Indicates that the command has been completed successfully.
    /// </summary>
    Completed = 11,

    /// <summary>
    /// Indicates that the command has been retried.
    /// </summary>
    Retried = 12,
}

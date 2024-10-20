namespace Willow.CommandAndControl.Data.Enums;

/// <summary>
/// Requested Command Status.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RequestedCommandStatus
{
    /// <summary>
    /// Command is new and no action has been taken.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Command has been approved.
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Command has been rejected.
    /// </summary>
    Rejected = 2,
}

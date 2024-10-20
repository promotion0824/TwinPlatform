namespace Willow.CommandAndControl.Data.Models;

/// <summary>
/// Stores application activity logs.
/// </summary>
public class ActivityLog
{
    /// <summary>
    /// Gets or sets the ID of the activity log.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the timestamp of the activity.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the activity type.
    /// </summary>
    public required ActivityType Type { get; set; }

    /// <summary>
    /// Gets or sets the foreign key of requested command.
    /// </summary>
    public required Guid RequestedCommandId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the resolved command, if applicable.
    /// </summary>
    public Guid? ResolvedCommandId { get; set; }

    /// <summary>
    /// Gets or sets any extra data associated with the activity.
    /// </summary>
    public string? ExtraInfo { get; set; }

    /// <summary>
    /// Gets or sets the user who carried out the activity.
    /// </summary>
    public User UpdatedBy { get; set; } = new();

    /// <summary>
    /// Gets or sets the navigation property of RequestedCommand entity.
    /// </summary>
    public RequestedCommand RequestedCommand { get; set; } = default!;

    /// <summary>
    /// Gets or sets the navigation property for the resolved command.
    /// </summary>
    public ResolvedCommand ResolvedCommand { get; set; } = default!;
}

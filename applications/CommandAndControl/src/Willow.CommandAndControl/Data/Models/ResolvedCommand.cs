namespace Willow.CommandAndControl.Data.Models;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents the commands resolved for execution.
/// </summary>
public class ResolvedCommand : BaseAuditableEntity
{
    /// <summary>
    /// Gets or sets the status of the resolved command.
    /// </summary>
    /// <remarks>
    /// Resolved Command will be created as Scheduled Status. User can mark the Scheduled commands as Cancelled. Depending on the action, status can be Executing, Executed, or Failed.
    /// </remarks>
    public required ResolvedCommandStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the foreign key of requested command.
    /// </summary>
    public required Guid RequestedCommandId { get; set; }

    /// <summary>
    /// Gets or sets the the actual time from which the value should be applied.
    /// </summary>
    public required DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Gets or sets the the time until other values cannot be applied.
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the any comment provided during the last status update.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the user who last updated the status.
    /// </summary>
    public User StatusUpdatedBy { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether the command has expired. A command is expired if it has not been executed successfully and interval has passed.
    /// </summary>
    [NotMapped]
    public bool IsExpired => (!EndTime.HasValue || DateTimeOffset.Now < EndTime.Value) &&
            (Status == ResolvedCommandStatus.Scheduled || Status == ResolvedCommandStatus.Failed ||
             Status == ResolvedCommandStatus.Cancelled);

    /// <summary>
    /// Gets or sets the navigation property of RequestedCommand entity.
    /// </summary>
    public RequestedCommand RequestedCommand { get; set; } = default!;
}

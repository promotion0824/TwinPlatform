namespace Willow.CommandAndControl.Data.Models;

/// <summary>
/// Represent command requested by source app.
/// </summary>
public class RequestedCommand : BaseAuditableEntity
{
    /// <summary>
    /// Gets or sets the incoming command name.
    /// </summary>
    public required string CommandName { get; set; }

    /// <summary>
    /// Gets or sets the command type.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Gets or sets the connector ID.
    /// </summary>
    public required string ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the twin ID.
    /// </summary>
    public required string TwinId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the point on the external system.
    /// </summary>
    /// <remarks>For Mapped this happens to be the same as the twinId, but that may not always be the case.</remarks>
    public required string ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the value for the command.
    /// </summary>
    /// <remarks>This may be the value that is set depending on the conflict resolution strategy.</remarks>
    public required double Value { get; set; }

    /// <summary>
    /// Gets or sets the unit of the value.
    /// </summary>
    public required string Unit { get; set; }

    /// <summary>
    /// Gets or sets the the time from which the value should be applied. This may correspond to an actual start time for an executed command but it might not if conflict resolution decides otherwise.
    /// </summary>
    public required DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Gets or sets the the time until other values cannot be applied. The actual end time depends on the conflict resolution strategy.
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the unique ID of a rule in Rules Engine.
    /// </summary>
    public required string RuleId { get; set; }

    /// <summary>
    /// Gets or sets the site ID.
    /// </summary>
    public required string SiteId { get; set; }

    /// <summary>
    /// Gets or sets the host asset ID.
    /// </summary>
    public required string IsHostedBy { get; set; }

    /// <summary>
    /// Gets or sets the twin of which this twin is a capability of.
    /// </summary>
    public required string IsCapabilityOf { get; set; }

    /// <summary>
    /// Gets or sets the location of the twin.
    /// </summary>
    public required string Location { get; set; }

    /// <summary>
    /// Gets or sets the status of the requested command.
    /// </summary>
    public RequestedCommandStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who approved/retracted the command.
    /// </summary>
    public User StatusUpdatedBy { get; set; } = new();

    /// <summary>
    /// Gets or sets the received time of the latest version of the request.
    /// </summary>
    public DateTimeOffset ReceivedDate { get; set; }

    /// <summary>
    /// Gets or sets the list of resolved commands.
    /// </summary>
    public ICollection<ResolvedCommand> ResolvedCommands { get; set; } = default!;

    /// <summary>
    /// Gets or sets the twin's location hierarchy.
    /// </summary>
    public ICollection<LocationTwin> Locations { get; set; } = [];
}

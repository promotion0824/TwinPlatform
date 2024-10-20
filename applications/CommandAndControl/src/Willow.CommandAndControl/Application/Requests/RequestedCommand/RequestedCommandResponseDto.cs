namespace Willow.CommandAndControl.Application.Requests.RequestedCommand;
/// <summary>
/// Response Dto for Requested Commands.
/// </summary>
public record RequestedCommandResponseDto : TwinInfoModel
{
    /// <summary>
    /// Gets the ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the requested command can be Pending when entered, approved to be resolved, and rejected.
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Gets a uniquely identifiable ID of the Rules Engine rule.
    /// </summary>
    public required string RuleId { get; init; }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    public required string CommandName { get; init; }

    /// <summary>
    /// Gets the command type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets the Connector ID.
    /// </summary>
    public required new string ConnectorId { get; init; }

    /// <summary>
    /// Gets the value for the command.
    /// </summary>
    /// <remarks>This may be the value that is set depending on the conflict resolution strategy.</remarks>
    public double Value { get; init; }

    /// <summary>
    /// Gets the time from which the value should be applied. This may correspond to an actual start time for an executed command but it might not if conflict resolution decides otherwise.
    /// </summary>
    public DateTimeOffset StartTime { get; init; }

    /// <summary>
    /// Gets the command end time.
    /// </summary>
    public DateTimeOffset? EndTime { get; init; }

    /// <summary>
    /// Gets the command created date.
    /// </summary>
    public DateTimeOffset CreatedDate { get; init; }

    /// <summary>
    /// Gets the received date of the command.
    /// </summary>
    public DateTimeOffset ReceivedDate { get; init; }

    /// <summary>
    /// Gets the user who last updated the status.
    /// </summary>
    public User? StatusUpdatedBy { get; init; }

    /// <summary>
    /// Gets the command last updated date.
    /// </summary>
    public required DateTimeOffset LastUpdated { get; init; }
}

namespace Willow.CommandAndControl.Application.Requests.ResolvedCommand;
/// <summary>
/// Response Dto for Resolved Commands.
/// </summary>
public record ResolvedCommandResponseDto : TwinInfoModel
{
    /// <summary>
    /// Gets the ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the command name.
    /// </summary>
    public required string CommandName { get; set; }

    /// <summary>
    /// Gets or sets the command type.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Gets a uniquely identifiable ID of RulesEngine rule.
    /// </summary>
    public required string RuleId { get; init; }

    /// <summary>
    /// Gets the description of a Source App.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ResolvedCommandStatus Status { get; init; }

    /// <summary>
    /// Gets the connector ID.
    /// </summary>
    public required new string ConnectorId { get; init; }

    /// <summary>
    /// Gets the value for the command.
    /// </summary>
    public double Value { get; init; }

    /// <summary>
    /// Gets the actual time from which the value should be applied.
    /// </summary>
    public DateTimeOffset StartTime { get; init; }

    /// <summary>
    /// Gets the time until other values cannot be applied.
    /// </summary>
    public DateTimeOffset? EndTime { get; init; }

    /// <summary>
    /// Gets or sets the comment provided during the last status update.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets the command created date.
    /// </summary>
    public required DateTimeOffset CreatedDate { get; init; }

    /// <summary>
    /// Gets the last updated by user.
    /// </summary>
    public User? StatusUpdatedBy { get; init; }

    /// <summary>
    /// Gets the command last updated date.
    /// </summary>
    public required DateTimeOffset LastUpdated { get; init; }

    /// <summary>
    /// Gets the requested command ID.
    /// </summary>
    public required Guid RequestId { get; init; }

    /// <summary>
    /// Gets or sets the list of allowed actions.
    /// </summary>
    public List<string> Actions { get; set; } = [];
}

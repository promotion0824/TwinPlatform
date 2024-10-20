namespace Willow.CommandAndControlAPI.SDK.Dtos;

/// <summary>
/// Dto with list of requested commands.
/// </summary>

public record PostRequestedCommandsDto
{
    /// <summary>
    /// Gets or sets list of commands.
    /// </summary>
    public required List<RequestedCommandDto> Commands { get; init; }
}

/// <summary>
/// Requested Command DTO.
/// </summary>
public record RequestedCommandDto
{
    /// <summary>
    /// Gets or sets connector Id.
    /// </summary>
    public required string ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets name of Command.
    /// </summary>
    public required string CommandName { get; set; }

    /// <summary>
    /// Gets or sets type of Command.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Gets or sets twin Id.
    /// </summary>
    public required string TwinId { get; set; }

    /// <summary>
    /// Gets or sets external Id from Mapped.
    /// </summary>
    public required string ExternalId { get; set; }

    /// <summary>
    /// Gets or sets rulesEngine Rule Id with which the command was generated.
    /// </summary>
    public required string RuleId { get; set; }

    /// <summary>
    /// Gets or sets value.
    /// </summary>
    public required double Value { get; set; }

    /// <summary>
    /// Gets or sets unit of given value.
    /// </summary>
    public required string Unit { get; set; }

    /// <summary>
    /// Gets or sets desired time to apply given value.
    /// </summary>
    public required DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Gets or sets desired end time.
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the twin relationships, including the ancestors.
    /// </summary>
    public IEnumerable<RelationshipDto> Relationships { get; set; } = [];
}

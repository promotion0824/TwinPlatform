namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.PostRequestedCommands;

/// <summary>
/// Creates new command.
/// </summary>
/// <param name="Commands">List of commands.</param>
public record PostRequestedCommandsDto(List<RequestedCommandDto> Commands);

/// <summary>
/// Post command detail.
/// </summary>
/// <param name="ConnectorId">Connector Id.</param>
/// <param name="CommandName">Name of Command.</param>
/// <param name="Type">Type of Command.</param>
/// <param name="TwinId">Twin Id.</param>
/// <param name="ExternalId">External Id from Mapped.</param>
/// <param name="RuleId">RulesEngine Rule Id with which the command was generated.</param>
/// <param name="Value">Value.</param>
/// <param name="Unit">Unit of given value.</param>
/// <param name="StartTime">Desired time to apply given value.</param>
/// <param name="EndTime">Desired end time.</param>
/// <param name="Relationships">This twin's relationships and ancestors.</param>
public record RequestedCommandDto(
    string ConnectorId,
    string CommandName,
    string Type,
    string TwinId,
    string ExternalId,
    string RuleId,
    double Value,
    string Unit,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    IEnumerable<RelationshipDto> Relationships);

/// <summary>
/// Represents a relationship twin.
/// </summary>
/// <param name="TwinId">The ID of the twin at the other end of the relationship.</param>
/// <param name="TwinName">The name of the twin at the other end of the relationship.</param>
/// <param name="ModelId">The ID of the model at the other end of the relationship.</param>
/// <param name="RelationshipType">The type of the relationship.</param>
public record RelationshipDto(string TwinId, string TwinName, string ModelId, string RelationshipType);

namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.GetConflictingCommands;

/// <summary>
/// Conflicting Commands Response Dto.
/// </summary>
public record ConflictingCommandsResponseDto : TwinInfoModel
{
    /// <summary>
    /// Gets a deterministic key of the conflicting command.
    /// </summary>
    public string Key => $"{ConnectorId}|{TwinId}|{IsCapabilityOf}|{IsHostedBy}|{Location}|{ExternalId}|{Unit}";

    /// <summary>
    /// Gets the date of the earliest received command.
    /// </summary>
    public required DateTimeOffset ReceivedDate { get; init; }

    /// <summary>
    /// Gets the number of commands.
    /// </summary>
    public int Commands { get; init; }

    /// <summary>
    /// Gets the number of approved commands.
    /// </summary>
    public int ApprovedCommands { get; init; }

    /// <summary>
    /// Gets or sets the ist of Requested Commands.
    /// </summary>
    public List<RequestedCommandResponseDto> Requests { get; set; } = [];
}

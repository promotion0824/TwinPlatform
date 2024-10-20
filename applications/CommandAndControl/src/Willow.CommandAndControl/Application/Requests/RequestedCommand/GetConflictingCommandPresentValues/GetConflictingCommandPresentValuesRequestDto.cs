namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.GetConflictingCommands;

/// <summary>
/// Get Conflicting Commands.
/// </summary>
public record GetConflictingCommandPresentValuesRequestDto
{
    /// <summary>
    /// Gets or sets external Ids.
    /// </summary>
    public required List<string> ExternalIds { get; set; }
}

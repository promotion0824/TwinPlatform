namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.GetConflictingCommands;

/// <summary>
/// Conflicting Commands Response Dto.
/// </summary>
public record GetConflictingCommandPresentValuesResponseDto
{
    /// <summary>
    /// Gets or Sets PresentValues.
    /// </summary>
    public Dictionary<string, double?> PresentValues { get; set; } = [];
}

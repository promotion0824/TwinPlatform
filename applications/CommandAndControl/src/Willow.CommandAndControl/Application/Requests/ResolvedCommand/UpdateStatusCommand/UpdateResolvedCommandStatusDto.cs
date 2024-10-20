namespace Willow.CommandAndControl.Application.Requests.ResolvedCommand.UpdateStatusCommand;

using Willow.CommandAndControl.Application.Helpers;

/// <summary>
/// Update ResolvedCommand Status.
/// </summary>
public record UpdateResolvedCommandStatusDto
{
    /// <summary>
    /// Gets the resource command action.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ResolvedCommandAction Action { get; init; }

    /// <summary>
    /// Gets any comment provided with the status update.
    /// </summary>
    public string? Comment { get; init; }
}

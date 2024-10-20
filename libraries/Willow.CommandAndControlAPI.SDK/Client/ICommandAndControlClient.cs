namespace Willow.CommandAndControlAPI.SDK.Client;

using Willow.CommandAndControlAPI.SDK.Dtos;

/// <summary>
/// Command and Control client.
/// </summary>
public interface ICommandAndControlClient
{
    /// <summary>
    /// Post commands to command and control app.
    /// </summary>
    /// <param name="postRequestedCommandsDto">PostRequestedCommandsDto.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task PostRequestedCommands(PostRequestedCommandsDto postRequestedCommandsDto, CancellationToken cancellationToken = default);
}

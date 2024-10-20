namespace Willow.CommandAndControlAPI.SDK.Client;

using System.Net.Http.Json;
using System.Text.Json;
using Willow.CommandAndControlAPI.SDK.Dtos;

/// <summary>
/// Command and control client.
/// </summary>
public class CommandAndControlClient(HttpClient client) : ICommandAndControlClient
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private const int MaxCommandsPerBatch = 200;

    /// <summary>
    /// Post requested commands in batches. Per batch contains 200 commands.
    /// </summary>
    /// <param name="postRequestedCommandsDto">Commands.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="AggregateException">Exception containing the exception per batch sent.</exception>
    /// <returns>Task.</returns>
    public async Task PostRequestedCommands(PostRequestedCommandsDto postRequestedCommandsDto, CancellationToken cancellationToken = default)
    {
        var batch = postRequestedCommandsDto.Commands.Chunk(MaxCommandsPerBatch);

        foreach (var commands in batch)
        {
            await PostAsync(new PostRequestedCommandsDto
                {
                    Commands = commands.ToList(),
                },
                cancellationToken);
        }
    }

    private async Task PostAsync(PostRequestedCommandsDto postRequestedCommandsDto, CancellationToken cancellationToken)
    {
        using var result = await client.PostAsJsonAsync("api/requested-commands", postRequestedCommandsDto, jsonSerializerOptions, cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new HttpRequestException(HttpRequestError.Unknown, responseContent, statusCode: result.StatusCode);
    }
}

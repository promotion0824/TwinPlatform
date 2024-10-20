namespace Willow.ServiceHealthAggregator.Snowflake.Options;

/// <summary>
/// Options for sending alerts via Teams.
/// </summary>
public record TeamsOptions
{
    /// <summary>
    /// Gets a value indicating whether to send a Teams message.
    /// </summary>
    public required bool SendTeamsMessage { get; init; }

    /// <summary>
    /// Gets the webhook URL for sending a Teams message.
    /// </summary>
    public required string WebhookUrl { get; init; }
}

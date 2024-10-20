namespace Willow.ServiceHealthAggregator.Snowflake.Options;

/// <summary>
/// Options for sending alerts via email.
/// </summary>
public record EmailOptions
{
    /// <summary>
    /// Gets a value indicating whether to send an email.
    /// </summary>
    public required bool SendEmail { get; init; }

    /// <summary>
    /// Gets the email address of the sender.
    /// </summary>
    public required string SenderEmail { get; init; }

    /// <summary>
    /// Gets the email address of the recipient.
    /// </summary>
    public required string RecipientEmail { get; init; }
}

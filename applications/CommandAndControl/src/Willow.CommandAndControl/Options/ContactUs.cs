namespace Willow.CommandAndControl.Options;

/// <summary>
/// Options for contact us.
/// </summary>
public record ContactUs
{
    /// <summary>
    /// Gets the email address to send contact us emails from.
    /// </summary>
    public required string FromEmail { get; init; }

    /// <summary>
    /// Gets a list of email addresses to send contact us emails to.
    /// </summary>
    public List<string> ToEmailAddresses { get; init; } = [];
}

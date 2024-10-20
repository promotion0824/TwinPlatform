namespace Authorization.TwinPlatform.Services.Hosted;

/// <summary>
/// Email Notification Service Option
/// </summary>
public record EmailNotificationServiceOption
{
    /// <summary>
    /// True to enable hosted service; false if not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Time span to dictate how frequently the host service should execute.
    /// </summary>
    public TimeSpan RefreshInterval { get; set; }

    /// <summary>
    /// SendGrid Email API key
    /// </summary>
    public string SendGridAPIKey { get; set; } = default!;

    /// <summary>
    /// Sender Email Address.
    /// </summary>
    public string SenderEmail { get; set; } = default!;

    /// <summary>
    /// Sender Name.
    /// </summary>
    public string SenderName { get; set;} = default!;
}

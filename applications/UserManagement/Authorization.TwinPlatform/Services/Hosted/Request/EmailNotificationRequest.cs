namespace Authorization.TwinPlatform.Services.Hosted.Request;

/// <summary>
/// Email Notification Request.
/// </summary>
public record EmailNotificationRequest : IBackgroundRequest
{
    /// <summary>
    /// Request Id.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Get the Unique Identifier
    /// </summary>
    /// <returns>Unique Identifier String.</returns>
    public string GetIdentifier() => Id;

    public (string Email, string Name) Receiver { get; set; } = default!;

    /// <summary>
    /// Template Identifier
    /// </summary>
    public string TemplateId { get; set; } = default!;

    /// <summary>
    /// Dynamic Data for the template.
    /// </summary>
    public Dictionary<string, object> TemplateData { get; set; } = [];
}

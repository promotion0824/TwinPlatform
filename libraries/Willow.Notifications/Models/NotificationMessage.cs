namespace Willow.Notifications.Models;

using Willow.Notifications.Enums;

/// <summary>
/// Notification Message Model.
/// This model is used with new notification core service.
/// </summary>
public class NotificationMessage
{
    /// <summary>
    /// Gets or sets the notification source.
    /// </summary>
    public NotificationSource NotificationSource { get; set; }

    /// <summary>
    /// Gets or sets the notification Title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification dynamic proprieties.
    /// </summary>
    public dynamic PropertyBagJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets SourceId, SourceId  is the id of the entity that this notification is associated with
    /// this id will be stored as a string, but it can represent any type of entity id
    /// and the type of entity will be determined by the Source property.
    /// </summary>
    public string SourceId { get; set; } = string.Empty;
}

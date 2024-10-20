namespace Willow.Notifications.Models;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Communication channel used to send the notification.
/// </summary>
public enum CommunicationType
{
    /// <summary>
    /// Notification sent via Email.
    /// </summary>
    [EnumMember(Value = "email")]
    Email,

    /// <summary>
    /// Notification sent via mobile notification.
    /// </summary>
    [EnumMember(Value = "pushnotification")]
    PushNotification,

    /// <summary>
    /// Notification sent via in app notification.
    /// </summary>
    InApp,
}

/// <summary>
/// Notification Model.
/// This model is used with legacy notification service (commsvc).
/// </summary>
public class Notification
{
    /// <summary>
    /// Gets or sets correlationId of the notification.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets customer Id.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets user Id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets user Type.
    /// </summary>
    public string UserType { get; set; } = default!;

    /// <summary>
    /// Gets or sets communication Type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public CommunicationType CommunicationType { get; set; }

    /// <summary>
    /// Gets or sets notification Locale.
    /// </summary>
    public string? Locale { get; set; }

    /// <summary>
    /// Gets or sets template Name.
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Gets or sets notification Message.
    /// </summary>
    public IDictionary<string, object>? Data { get; set; }

    /// <summary>
    /// Gets or sets notification Tags.
    /// </summary>
    public IDictionary<string, object>? Tags { get; set; }
}

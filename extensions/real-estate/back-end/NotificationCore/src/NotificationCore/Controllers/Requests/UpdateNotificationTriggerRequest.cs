using System;
using NotificationCore.Models;

namespace NotificationCore.Controllers.Requests;

public class UpdateNotificationTriggerRequest: NotificationTriggerRequestBase
{
    /// <summary>
    /// Notification type (Personal/ Workgroup)
    /// </summary>
    public NotificationType? Type { get; set; }
    /// <summary>
    /// Notification source, ex: Insight
    /// </summary>
    public NotificationSource? Source { get; set; }
    /// <summary>
    /// Notification trigger's focus, ex: twin, skill, twin category, skill category
    /// </summary>
    public NotificationFocus? Focus { get; set; }
    /// <summary>
    /// Toggle  to enable or disable the notification trigger
    /// </summary>
    public bool? IsEnabled { get; set; }
    /// <summary>
    /// Toggle to enable or disable the notification trigger for user
    /// </summary>
    public bool? IsEnabledForUser { get; set; }
    /// <summary>
    /// the updated user id
    /// </summary>
    public Guid UpdatedBy { get; set; }
    /// <summary>
    /// Represents if user can disable a notification trigger
    /// </summary>
    public bool? CanUserDisableNotification { get; set; }
    /// <summary>
    /// Represent if the notification trigger is set to all location
    /// </summary>
    public bool? AllLocation { get; set; }
}

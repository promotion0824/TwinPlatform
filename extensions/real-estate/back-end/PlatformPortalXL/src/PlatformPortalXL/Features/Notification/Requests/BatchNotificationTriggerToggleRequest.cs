using PlatformPortalXL.Models.NotificationTrigger;

namespace PlatformPortalXL.Features.Notification.Requests;
public class BatchNotificationTriggerToggleRequest
{
    /// <summary>
    /// Notification trigger source, ex: Insight
    /// </summary>
    public NotificationTriggerSource Source { get; set; }

}

using PlatformPortalXL.Models.NotificationTrigger;
using System;
using System.Collections.Generic;

namespace PlatformPortalXL.ServicesApi.NotificationTriggerApi.Request;

public class BatchNotificationTriggerToggleApiRequest
{
    /// <summary>
    /// Notification source, ex: Insight
    /// </summary>
    public NotificationTriggerSource Source { get; set; }
    /// <summary>
    /// The user id who is toggling the notification trigger
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// It represents if user can act as a customer admin
    /// </summary>
    public bool IsAdmin { get; set; }
    /// <summary>
    /// List of user's workgroup ids
    /// </summary>
    public List<Guid> WorkgroupIds { get; set; }
}

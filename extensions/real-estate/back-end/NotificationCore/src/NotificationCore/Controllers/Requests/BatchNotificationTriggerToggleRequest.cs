using System.Collections.Generic;
using System;
using NotificationCore.Models;

namespace NotificationCore.Controllers.Requests;

public class BatchNotificationTriggerToggleRequest
{
    /// <summary>
    /// Notification source, ex: Insight
    /// </summary>
    public NotificationSource Source { get; set; }
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

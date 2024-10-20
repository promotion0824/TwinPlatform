using System.Collections.Generic;
using System;
using PlatformPortalXL.Models.Notification;

namespace PlatformPortalXL.Features.Notification.Requests;
public class UpdateNotificationStateRequest
{
    /// <summary>
    /// The ids of the notifications to be updated
    /// </summary>
    public List<Guid> NotificationIds { get; set; }

    /// <summary>
    /// The requesed notification state
    /// </summary>
    public NotificationUserState State { get; set; }
}

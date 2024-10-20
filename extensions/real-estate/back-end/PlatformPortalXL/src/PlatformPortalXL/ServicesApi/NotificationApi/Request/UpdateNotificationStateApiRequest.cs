using System.Collections.Generic;
using System;
using PlatformPortalXL.Models.Notification;

namespace PlatformPortalXL.ServicesApi.NotificationApi.Request;

public class UpdateNotificationStateApiRequest
{
    /// <summary>
    /// The id of the user that received the notifications
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The ids of the notifications to be updated
    /// </summary>
    public List<Guid> NotificationIds { get; set; }

    /// <summary>
    /// The requesed notification state
    /// </summary>
    public NotificationUserState State { get; set; }
}

using System;

namespace PlatformPortalXL.Models.NotificationTrigger;

public class NotificationSubscriptionOverride
{
    public Guid UserId { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsMuted { get; set; }
}

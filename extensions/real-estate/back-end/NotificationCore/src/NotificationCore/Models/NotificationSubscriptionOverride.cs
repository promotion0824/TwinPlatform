using System;

namespace NotificationCore.Models;
public class NotificationSubscriptionOverride
{
    public Guid UserId { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsMuted { get; set; }

}

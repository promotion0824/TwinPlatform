using System;
using System.Collections.Generic;

namespace NotificationCore.Models;

public class NotificationTrigger
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public NotificationSource Source { get; set; }
    public NotificationFocus Focus { get; set; }
    public List<NotificationChannel> Channels { get; set; }
    public List<string> Locations { get; set; }
    public bool IsEnabled { get; set; }
    public bool? IsEnabledForUser { get; set; }
    public bool CanUserDisableNotification { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public List<Guid> WorkgroupIds { get; set; }
    public List<NotificationSubscriptionOverride> NotificationSubscriptionOverrides { get; set; }
    public List<int> PriorityIds { get; set; }
    public List<int> SkillCategoryIds { get; set; }
    public List<string> TwinCategoryIds { get; set; }
    public List<NotificationTriggerTwin> Twins { get; set; }
    public List<string> SkillIds { get; set; }
    public bool IsMuted { get; set; }
}

using System.Collections.Generic;
using System;

namespace PlatformPortalXL.Models.NotificationTrigger;

public class NotificationTrigger
{
    public Guid Id { get; set; }
    public NotificationTriggerType Type { get; set; }
    public NotificationTriggerSource Source { get; set; }
    public NotificationTriggerFocus Focus { get; set; }
    public List<NotificationTriggerChannel> Channels { get; set; }
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

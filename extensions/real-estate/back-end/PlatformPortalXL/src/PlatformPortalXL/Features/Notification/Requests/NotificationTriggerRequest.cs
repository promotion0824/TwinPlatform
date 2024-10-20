using System.Collections.Generic;
using System;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Models.NotificationTrigger;

namespace PlatformPortalXL.Features.Notification.Requests;
public class NotificationTriggerRequestBase
{
    /// <summary>
    /// Notification trigger's locations
    /// </summary>
    public List<string> Locations { get; set; }
    /// <summary>
    /// List of workgroup Ids
    /// </summary>
    public List<Guid> WorkGroupIds { get; set; }
    /// <summary>
    /// List of skill categories' ids (the skill's category is insight type)
    /// </summary>
    public List<int> SkillCategories { get; set; }
    /// <summary>
    /// List of twin categories' ids (the twin category is the twin's model Id)
    /// </summary>
    public List<string> TwinCategoryIds { get; set; }
    /// <summary>
    /// List of twin Ids
    /// </summary>
    public List<NotificationTriggerTwinDto> Twins { get; set; }
    /// <summary>
    /// List of skill ids
    /// </summary>
    public List<string> SkillIds { get; set; }
    /// <summary>
    /// List of priority Ids, ex: urgent, high
    /// </summary>
    public List<int> Priorities { get; set; }
    /// <summary>
    /// Notification trigger channel, ex: InApp
    /// </summary>
    public List<NotificationTriggerChannel> Channels { get; set; }
}

public class CreateNotificationTriggerRequest: NotificationTriggerRequestBase
{
    /// <summary>
    /// Notification type (Personal/ Workgroup)
    /// </summary>
    public NotificationTriggerType Type { get; set; }
    /// <summary>
    /// Notification source, ex: Insight
    /// </summary>
    public NotificationTriggerSource Source { get; set; }
    /// <summary>
    /// Notification trigger's focus, ex: twin, skill, twin category, skill category
    /// </summary>
    public NotificationTriggerFocus Focus { get; set; }

    /// <summary>
    /// Toggle  to enable and disable the notification trigger
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// Represents if user can disable a notification trigger
    /// </summary>
    public bool CanUserDisableNotification { get; set; }
}

public class UpdateNotificationTriggerRequest : NotificationTriggerRequestBase
{
    /// <summary>
    /// Notification type (Personal/ Workgroup)
    /// </summary>
    public NotificationTriggerType? Type { get; set; }
    /// <summary>
    /// Notification source, ex: Insight
    /// </summary>
    public NotificationTriggerSource? Source { get; set; }
    /// <summary>
    /// Notification trigger's focus, ex: twin, skill, twin category, skill category
    /// </summary>
    public NotificationTriggerFocus? Focus { get; set; }
    /// <summary>
    /// Toggle  to enable or disable the notification trigger
    /// </summary>
    public bool? IsEnabled { get; set; }
    /// <summary>
    /// Toggle to enable or disable the notification trigger for user
    /// </summary>
    public bool? IsEnabledForUser { get; set; }
    /// <summary>
    /// Represents if user can disable a notification trigger
    /// </summary>
    public bool? CanUserDisableNotification { get; set; }
    /// <summary>
    /// Represent if the notification trigger is set to all location
    /// </summary>
    public bool? AllLocation { get; set; }
}

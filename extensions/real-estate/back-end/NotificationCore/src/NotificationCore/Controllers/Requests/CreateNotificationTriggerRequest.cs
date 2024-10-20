using System;
using System.Collections.Generic;
using NotificationCore.Dto;
using NotificationCore.Models;

namespace NotificationCore.Controllers.Requests;
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
    public List<int> SkillCategoryIds { get; set; }
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
    public List<int> PriorityIds { get; set; }
    /// <summary>
    /// Notification trigger channel, ex: InApp
    /// </summary>
    public List<NotificationChannel> Channels { get; set; }
}
/// <summary>
/// Create notification trigger request
/// </summary>
public class CreateNotificationTriggerRequest: NotificationTriggerRequestBase
{
    /// <summary>
    /// Notification type (Personal/ Workgroup)
    /// </summary>
    public NotificationType Type { get; set; }
    /// <summary>
    /// Notification source, ex: Insight
    /// </summary>
    public NotificationSource Source { get; set; }
    /// <summary>
    /// Notification trigger's focus, ex: twin, skill, twin category, skill category
    /// </summary>
    public NotificationFocus Focus { get; set; }

    /// <summary>
    /// Toggle  to enable and disable the notification trigger
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// the created user id
    /// </summary>
    public Guid CreatedBy { get; set; }
    /// <summary>
    /// Represents if user can disable a notification trigger
    /// </summary>
    public bool CanUserDisableNotification { get; set; }

}

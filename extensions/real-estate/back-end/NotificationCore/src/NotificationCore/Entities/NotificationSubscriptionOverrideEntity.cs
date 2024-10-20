using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NotificationCore.Models;

namespace NotificationCore.Entities;

/// <summary>
/// This entity is used to store the notification subscription overrides for a user
/// When user is a  member of a workgroup with certain configuration, the users can override these configuration for themselves
/// </summary>
[Table("NotificationSubscriptionOverrides")]
[PrimaryKey(nameof(UserId), nameof(NotificationTriggerId))]
public class NotificationSubscriptionOverrideEntity
{
    /// <summary>
    /// The user id of the user that this override is for
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The id of the notification trigger that this override is for
    /// </summary>
    public Guid NotificationTriggerId { get; set; }

    /// <summary>
    /// The override value for the IsEnabled property in the notification trigger
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// The override value for the IsMuted property in the notification trigger
    /// </summary>
    public bool IsMuted { get; set; }
    [ForeignKey(nameof(NotificationTriggerId))]
    public NotificationTriggerEntity NotificationTrigger { get; set; }

    internal static List<NotificationSubscriptionOverride> MapTo(IEnumerable<NotificationSubscriptionOverrideEntity> models)
    {
        return models?.Select(c => new NotificationSubscriptionOverride
        {
            IsEnabled = c.IsEnabled,
            UserId = c.UserId
        }).ToList();
    }

    public static List<NotificationSubscriptionOverrideEntity> MapFrom(List<NotificationSubscriptionOverride>? modelNotificationSubscriptionOverrides)
    {
        return modelNotificationSubscriptionOverrides?.Select(c => new NotificationSubscriptionOverrideEntity
        {
            UserId = c.UserId,
            IsEnabled = c.IsEnabled
        }).ToList();
    }
}

using PlatformPortalXL.Models.NotificationTrigger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Dto;

public class NotificationSubscriptionOverrideDto
{
    public Guid UserId { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsMuted { get; set; }

    public static NotificationSubscriptionOverrideDto MapFrom(NotificationSubscriptionOverride model)
    {
        return model!= null? new NotificationSubscriptionOverrideDto
        {
            UserId = model.UserId,
            IsEnabled = model.IsEnabled,
            IsMuted = model.IsMuted
        }:null;
    }
    public static List<NotificationSubscriptionOverrideDto> MapFrom(List<NotificationSubscriptionOverride> models)
    {
        return models?.Select(MapFrom).ToList();
    }

}

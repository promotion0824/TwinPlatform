using System;
using System.Collections.Generic;
using System.Linq;
using NotificationCore.Models;

namespace NotificationCore.Dto;

public class NotificationSubscriptionOverrideDto
{
    public Guid UserId { get; set; }
    public bool IsEnabled { get; set; }
    public static List<NotificationSubscriptionOverrideDto> MapFrom(List<NotificationSubscriptionOverride> modelTwins)
    {
        return modelTwins?.Select(c => new NotificationSubscriptionOverrideDto
        {
            UserId = c.UserId,
            IsEnabled = c.IsEnabled
        }).ToList();
    }
}

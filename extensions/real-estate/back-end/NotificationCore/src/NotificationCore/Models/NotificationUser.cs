using System;
using System.Collections.Generic;
using System.Linq;
using NotificationCore.Entities;

namespace NotificationCore.Models;

public class NotificationUser
{
    public Guid UserId { get; set; }
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; }
    public NotificationUserState State { get; set; }
    public DateTime? ClearedDateTime { get; set; }

    public static NotificationUser MapFrom(NotificationUserEntity entity)
    {
        if (entity == null) return null;

        return new NotificationUser()
        {
            ClearedDateTime = entity.ClearedDateTime,
            Notification = Notification.MapFrom(entity.Notification),
            NotificationId = entity.NotificationId,
            State = entity.State,
            UserId = entity.UserId
        };
    }

    public static List<NotificationUser> MapFrom(IEnumerable<NotificationUserEntity> entities)
    {
        return entities?.Select(MapFrom).ToList();
    }
}


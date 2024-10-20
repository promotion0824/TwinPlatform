using System;
using System.Collections.Generic;
using System.Linq;
using NotificationCore.Models;

namespace NotificationCore.Dto;

public class NotificationUserDto
{
    public Guid Id { get; set; }
    public NotificationSource Source { get; set; }
    public string Title { get; set; }
    public string PropertyBagJson { get; set; }
    public Guid UserId { get; set; }
    public NotificationUserState State { get; set; }
    public DateTime? ClearedDateTime { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public string SourceId { get; set; }

    public static NotificationUserDto MapFrom(NotificationUser model)
    {
        if (model == null) return null;

        return new NotificationUserDto
        {
            Id = model.Notification.Id,
            Source = model.Notification.Source,
            Title = model.Notification.Title,
            CreatedDateTime = model.Notification.CreatedDateTime,
            ClearedDateTime = model.ClearedDateTime,
            State = model.State,
            UserId = model.UserId,
            PropertyBagJson = model.Notification.PropertyBagJson,
            SourceId = model.Notification.SourceId
        };
    }

    public static List<NotificationUserDto> MapFrom(IEnumerable<NotificationUser> models)
    {
        return models?.Select(MapFrom).ToList();
    }
}

using System.Collections.Generic;
using System;
using NotificationCore.Entities;
using System.Linq;

namespace NotificationCore.Models;

public class Notification
{
    public Guid Id { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public NotificationSource Source { get; set; }
    /// <summary>
    /// SourceId  is the id of the entity that this notification is associated with
    /// this id will be stored as a string, but it can represent any type of entity id
    /// and the type of entity will be determined by the Source property.
    /// </summary>
    public string SourceId { get; set; }
    public string Title { get; set; }
    public string PropertyBagJson { get; set; }
    public List<Guid> TriggerIds { get; set; }
    public List<Guid> UserIds { get; set; }

    public static Notification MapFrom(NotificationEntity entity)
    {
        if (entity == null) return null;

        return new Notification()
        {
            Id = entity.Id,
            CreatedDateTime = entity.CreatedDateTime,
            PropertyBagJson = entity.PropertyBagJson,
            Source = entity.Source,
            Title = entity.Title,
            TriggerIds = entity.TriggerIdsJson,
            SourceId = entity.SourceId,
        };
    }

    public static List<Notification> MapFrom(IEnumerable<NotificationEntity> entities)
    {
        return entities?.Select(MapFrom).ToList();
    }
}


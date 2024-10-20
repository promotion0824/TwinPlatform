using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Willow.Common;

namespace NotificationCore.Models;

public class EventMessageNotification
{
    /// <summary>
    /// the notification source.
    /// </summary>
    public NotificationSource NotificationSource { get; set; }

    /// <summary>
    /// Notification Title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///Notification dynamic proprieties.
    /// </summary>
    public ExpandoObject PropertyBagJson { get; set; }

    /// <summary>
    /// SourceId  is the id of the entity that this notification is associated with
    /// this id will be stored as a string, but it can represent any type of entity id
    /// and the type of entity will be determined by the Source property.
    /// </summary>
    public string SourceId { get; set; }

    public static NotificationMessage MapTo(EventMessageNotification eventMessageNotification)
    {
        if (eventMessageNotification is null)
            return null;
        var notificationMessage = new NotificationMessage();
        notificationMessage.SourceId = eventMessageNotification.SourceId;
        notificationMessage.Source = eventMessageNotification.NotificationSource;
        notificationMessage.Title = eventMessageNotification.Title;

        var dynamicProperties = eventMessageNotification.PropertyBagJson as IDictionary<string, object>;
        if (dynamicProperties is not null)
        {
            var propertyDict = new Dictionary<string, object>(dynamicProperties, StringComparer.OrdinalIgnoreCase);
            notificationMessage.TwinId = propertyDict.ContainsKey(nameof(NotificationMessage.TwinId)) ?
                propertyDict[nameof(NotificationMessage.TwinId)].ToString() : default;

            notificationMessage.TwinName = propertyDict.ContainsKey(nameof(NotificationMessage.TwinName)) ?
                propertyDict[nameof(NotificationMessage.TwinName)].ToString() : default;

            notificationMessage.ModelId = propertyDict.ContainsKey(nameof(NotificationMessage.ModelId)) ?
                propertyDict[nameof(NotificationMessage.ModelId)].ToString() : default;

            notificationMessage.SkillId = propertyDict.ContainsKey(nameof(NotificationMessage.SkillId)) ?
                propertyDict[nameof(NotificationMessage.SkillId)].ToString() : default;

            notificationMessage.SkillCategoryId = propertyDict.ContainsKey(nameof(NotificationMessage.SkillCategoryId)) ?
                int.TryParse(propertyDict[nameof(NotificationMessage.SkillCategoryId)].ToString(), out var skillCategoryId) ? skillCategoryId : default
                : default;

            notificationMessage.Priority = propertyDict.ContainsKey(nameof(NotificationMessage.Priority)) ?
                int.TryParse(propertyDict[nameof(NotificationMessage.Priority)].ToString(), out var priority) ? priority : default
                : default;

            notificationMessage.Locations = propertyDict.ContainsKey(nameof(NotificationMessage.Locations)) ?
                propertyDict[nameof(NotificationMessage.Locations)].ToObjectList().OfType<string>().ToList() : default;
        }


        return notificationMessage;


    }
}


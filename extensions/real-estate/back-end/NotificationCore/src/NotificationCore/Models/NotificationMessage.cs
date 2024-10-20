using System;
using System.Collections.Generic;

namespace NotificationCore.Models;

public class NotificationMessage
{
    public NotificationMessage()
    {
        Locations = [];
    }
    /// <summary>
    /// SourceId  is the id of the entity that this notification is associated with
    /// this id will be stored as a string, but it can represent any type of entity id
    /// and the type of entity will be determined by the Source property.
    /// </summary>
    public string SourceId { get; set; }
    /// <summary>
    /// Source is the type of entity that this notification is associated with
    /// </summary>
    public NotificationSource Source { get; set; }
    /// <summary>
    /// Title of the notification
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// TwinId associated with the notification
    /// </summary>
    public string TwinId { get; set; }
    /// <summary>
    /// TwinName associated with the notification
    /// </summary>
    public string TwinName { get; set; }
    /// <summary>
    /// Twin ModelId associated with the notification
    /// </summary>
    public string ModelId { get; set; }

    /// <summary>
    /// SkillCategoryId associated with the notification
    /// this same as Insight Type in insight service
    /// </summary>
    public int? SkillCategoryId { get; set; }
    /// <summary>
    /// SkillId associated with the notification
    /// this is same as RuleId in insight service
    /// </summary>
    public string SkillId { get; set; }
    /// <summary>
    /// Priority of the notification
    /// This value associated with entity where we created notification from
    /// </summary>
    public int? Priority { get; set; }
    /// <summary>
    /// Locations associated with the notification
    /// this is list of string representing the twin ids of the locations of the entity where we created notification from
    /// </summary>
    public List<string> Locations { get; set; }
}


using NotificationCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotificationCore.Entities;

[Table("Notifications")]
public class NotificationEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedDateTime { get; private set; } = DateTime.UtcNow;
    public NotificationSource Source { get; set; }
    [MaxLength(512)]
    public string Title { get; set; }

    /// <summary>
    /// PropertyBagJson is a json string that contains the properties of the notification
    /// </summary>
    public string PropertyBagJson { get; set; }
    /// <summary>
    /// this is a list of trigger ids that are associated with this notification
    /// it will be stored as an array of Guids
    /// </summary>
    public List<Guid> TriggerIdsJson { get; set; }

    /// <summary>
    /// SourceId is the id of the entity that this notification is associated with
    /// this id will be stored as a string, but it can represent any type of entity id
    /// and the type of entity will be determined by the Source property
    /// </summary>
    [MaxLength(250)]
    public string SourceId { get; set; }
}


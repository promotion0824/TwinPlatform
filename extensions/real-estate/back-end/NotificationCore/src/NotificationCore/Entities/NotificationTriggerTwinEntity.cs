using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using NotificationCore.Models;

namespace NotificationCore.Entities;

[Table("NotificationTriggerTwins")]
[PrimaryKey(nameof(TwinId), nameof(NotificationTriggerId))]
public class NotificationTriggerTwinEntity
{
    public Guid NotificationTriggerId { get; set; }
    [Required(AllowEmptyStrings = false)]
    [MaxLength(250)]
    public string TwinId { get; set; }
    public string TwinName { get; set; }
    [ForeignKey(nameof(NotificationTriggerId))]
    public NotificationTriggerEntity NotificationTrigger { get; set; }

    internal static List<NotificationTriggerTwin> MapTo(IEnumerable<NotificationTriggerTwinEntity> twins)
    {
        return twins?.Select(c => new NotificationTriggerTwin
        {
            TwinId = c.TwinId,
            TwinName = c.TwinName
        }).ToList();
    }

    public static List<NotificationTriggerTwinEntity> MapFrom(List<NotificationTriggerTwin> modelTwins)
    {
        return modelTwins?.Select(c => new NotificationTriggerTwinEntity
        {
            TwinId = c.TwinId,
            TwinName = c.TwinName
        }).ToList();
    }
}

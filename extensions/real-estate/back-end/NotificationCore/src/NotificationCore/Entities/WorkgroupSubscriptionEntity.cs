using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace NotificationCore.Entities;

[Table("WorkgroupSubscriptions")]
[PrimaryKey(nameof(WorkgroupId), nameof(NotificationTriggerId))]
public class WorkgroupSubscriptionEntity
{
    public Guid NotificationTriggerId { get; set; }
    public Guid WorkgroupId { get; set; }
    [ForeignKey(nameof(NotificationTriggerId))]
    public NotificationTriggerEntity NotificationTrigger { get; set; }

    internal static List<Guid> MapTo(IEnumerable<WorkgroupSubscriptionEntity> workgroupSubscriptions)
    {
        return workgroupSubscriptions?.Select(c => c.WorkgroupId).ToList();
    }

    public static List<WorkgroupSubscriptionEntity> MapFrom(List<Guid> modelWorkgroupIds)
    {
        return modelWorkgroupIds?.Select(c => new WorkgroupSubscriptionEntity
        {
            WorkgroupId = c
        }).ToList();
    }
}

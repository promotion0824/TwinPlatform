using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.ComponentModel.DataAnnotations;


namespace NotificationCore.Entities;

[Table("NotificationTriggerTwinCategories")]
[PrimaryKey(nameof(CategoryId), nameof(NotificationTriggerId))]
public class NotificationTriggerTwinCategoryEntity
{
    public Guid NotificationTriggerId { get; set; }

    /// <summary>
    /// Twin category id is the twin model id
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MaxLength(250)]
    public string CategoryId { get; set; }
    [ForeignKey(nameof(NotificationTriggerId))]
    public NotificationTriggerEntity NotificationTrigger { get; set; }

    internal static List<string> MapTo(IEnumerable<NotificationTriggerTwinCategoryEntity> categories)
    {
        return categories?.Select(c => c.CategoryId).ToList();
    }

    public static List<NotificationTriggerTwinCategoryEntity> MapFrom(List<string> modelCategoryIds)
    {
        return modelCategoryIds?.Select(c => new NotificationTriggerTwinCategoryEntity
        {
            CategoryId = c
        }).ToList();
    }
}

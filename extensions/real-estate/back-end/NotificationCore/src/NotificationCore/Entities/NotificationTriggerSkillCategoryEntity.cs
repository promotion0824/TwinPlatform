using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;


namespace NotificationCore.Entities;

[Table("NotificationTriggerSkillCategories")]
[PrimaryKey(nameof(CategoryId), nameof(NotificationTriggerId))]
public class NotificationTriggerSkillCategoryEntity
{
    public Guid NotificationTriggerId { get; set; }

    /// <summary>
    /// Skill category id is the insight type
    /// </summary>
    public int CategoryId { get; set; }
    [ForeignKey(nameof(NotificationTriggerId))]
    public NotificationTriggerEntity NotificationTrigger { get; set; }

    internal static List<int> MapTo(IEnumerable<NotificationTriggerSkillCategoryEntity> categories)
    {
        return categories?.Select(c => c.CategoryId).ToList();
    }

    public static List<NotificationTriggerSkillCategoryEntity> MapFrom(List<int> modelCategoryIds)
    {
        return modelCategoryIds?.Select(c => new NotificationTriggerSkillCategoryEntity
        {
            CategoryId = c
        }).ToList();
    }
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace NotificationCore.Entities;

[Table("NotificationTriggerSkills")]
[PrimaryKey(nameof(SkillId), nameof(NotificationTriggerId))]
public class NotificationTriggerSkillEntity
{
    public Guid NotificationTriggerId { get; set; }

    /// <summary>
    /// Skill id is the insight Rule Id
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MaxLength(450)]
    public string SkillId { get; set; }
    [ForeignKey(nameof(NotificationTriggerId))]
    public NotificationTriggerEntity NotificationTrigger { get; set; }

    internal static List<string> MapTo(IEnumerable<NotificationTriggerSkillEntity> skills)
    {
        return skills?.Select(c => c.SkillId).ToList();
    }

    public static List<NotificationTriggerSkillEntity>? MapFrom(List<string>? modelSkillIds)
    {
        return modelSkillIds?.Select(c => new NotificationTriggerSkillEntity
        {
            SkillId = c
        }).ToList();
    }
}

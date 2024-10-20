using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using NotificationCore.Models;

namespace NotificationCore.Entities;

[Table("NotificationTriggers")]
public class NotificationTriggerEntity
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public NotificationSource Source { get; set; }
    public NotificationFocus Focus { get; set; }
    [Required(AllowEmptyStrings = false)]
    [MaxLength(250)]
    public List<NotificationChannel> ChannelJson { get; set; }
    [MaxLength(250)]
    public List<int> PriorityJson { get; set; }

    /// <summary>
    /// Indicate if the notification is enabled for the user/workgroup
    /// </summary>
    public bool IsEnabled { get; set; }
    public bool CanUserDisableNotification { get; set; } 
    public Guid CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsDefault { get; set; }
    public Guid? DerivedFrom { get; set; }

    /// <summary>
    /// if this is true, the notification will not be sent to the user
    /// We only send notifications if the notification trigger is enabled and not muted
    /// </summary>
    public bool IsMuted { get; set; }
    [ForeignKey(nameof(DerivedFrom))]
    public NotificationTriggerEntity DerivedFromTrigger { get; set; }
    public ICollection<LocationEntity> Locations { get; set; }
    public ICollection<WorkgroupSubscriptionEntity> WorkgroupSubscriptions { get; set; }
    public ICollection<NotificationSubscriptionOverrideEntity> NotificationSubscriptionOverrides { get; set; }
    public ICollection<NotificationTriggerSkillCategoryEntity> SkillCategories { get; set; }
    public ICollection<NotificationTriggerTwinCategoryEntity> TwinCategories { get; set; }
    public ICollection<NotificationTriggerSkillEntity> Skills { get; set; }
    public ICollection<NotificationTriggerTwinEntity> Twins { get; set; }

    public static NotificationTrigger MapTo(NotificationTriggerEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new NotificationTrigger
        {
            Id = entity.Id,
            Type = entity.Type,
            Source = entity.Source,
            Focus = entity.Focus,
            Locations = LocationEntity.MapTo(entity.Locations),
            IsEnabled = entity.IsEnabled,
            IsEnabledForUser = !entity.NotificationSubscriptionOverrides?.Any(),
            CanUserDisableNotification = entity.CanUserDisableNotification,
            CreatedBy = entity.CreatedBy,
            CreatedDate = entity.CreatedDate,
            UpdatedBy = entity.UpdatedBy,
            UpdatedDate = entity.UpdatedDate,
            Channels = entity.ChannelJson ,
            WorkgroupIds = WorkgroupSubscriptionEntity.MapTo(entity.WorkgroupSubscriptions),
            NotificationSubscriptionOverrides = NotificationSubscriptionOverrideEntity.MapTo(entity.NotificationSubscriptionOverrides),
            PriorityIds = entity.PriorityJson,
            SkillCategoryIds = NotificationTriggerSkillCategoryEntity.MapTo(entity.SkillCategories),
            TwinCategoryIds = NotificationTriggerTwinCategoryEntity.MapTo(entity.TwinCategories),
            SkillIds = NotificationTriggerSkillEntity.MapTo(entity.Skills),
            Twins = NotificationTriggerTwinEntity.MapTo(entity.Twins),
            IsMuted = entity.IsMuted
        };
    }

    public static IEnumerable<NotificationTrigger> MapTo(IEnumerable<NotificationTriggerEntity> entities)
    {
        return entities?.Select(MapTo).ToList();
    }

    public static NotificationTriggerEntity MapFrom(NotificationTrigger model)
    {
        if(model is null)
        {
            return null;
        }
        return new NotificationTriggerEntity
        {
            Id = model.Id,
            Type = model.Type,
            Source = model.Source,
            Focus = model.Focus,
            ChannelJson = model.Channels,
            IsEnabled = model.IsEnabled,
            CanUserDisableNotification = model.CanUserDisableNotification,
            CreatedBy = model.CreatedBy,
            CreatedDate = model.CreatedDate,
            UpdatedBy = model.UpdatedBy,
            UpdatedDate = model.UpdatedDate,
            Locations = LocationEntity.MapFrom(model.Locations),
            WorkgroupSubscriptions = WorkgroupSubscriptionEntity.MapFrom(model.WorkgroupIds),
            NotificationSubscriptionOverrides = NotificationSubscriptionOverrideEntity.MapFrom(model.NotificationSubscriptionOverrides),
            PriorityJson = model.PriorityIds,
            SkillCategories = NotificationTriggerSkillCategoryEntity.MapFrom(model.SkillCategoryIds),
            TwinCategories = NotificationTriggerTwinCategoryEntity.MapFrom(model.TwinCategoryIds),
            Skills = NotificationTriggerSkillEntity.MapFrom(model.SkillIds),
            Twins = NotificationTriggerTwinEntity.MapFrom(model.Twins),
            IsMuted = model.IsMuted
        };
    }
}


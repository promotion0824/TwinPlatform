using PlatformPortalXL.Models.NotificationTrigger;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PlatformPortalXL.Dto;

public class NotificationTriggerDto
{
    public Guid Id { get; set; }
    public NotificationTriggerType Type { get; set; }
    public NotificationTriggerSource Source { get; set; }
    public NotificationTriggerFocus Focus { get; set; }
    public List<NotificationTriggerChannel> Channels { get; set; }
    public List<string> Locations { get; set; }
    public bool IsEnabled { get; set; }
    public bool? IsEnabledForUser { get; set; }
    public bool CanUserDisableNotification { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public List<Guid> WorkgroupIds { get; set; }
    public List<NotificationSubscriptionOverrideDto> NotificationSubscriptionOverrides { get; set; }
    public List<int> PriorityIds { get; set; }
    public List<int> SkillCategoryIds { get; set; }
    public List<string> TwinCategoryIds { get; set; }
    public List<NotificationTriggerTwinDto> Twins { get; set; }
    public List<string> SkillIds { get; set; }
    public bool IsMuted { get; set; }

    public static NotificationTriggerDto MapFrom(NotificationTrigger model)
    {
        return model!=null? new NotificationTriggerDto
        {
            Id = model.Id,
            Type = model.Type,
            Source = model.Source,
            Focus = model.Focus,
            Locations = model.Locations,
            IsEnabled = model.IsEnabled,
            IsEnabledForUser = model.IsEnabledForUser,
            CanUserDisableNotification = model.CanUserDisableNotification,
            CreatedBy = model.CreatedBy,
            CreatedDate = model.CreatedDate,
            UpdatedBy = model.UpdatedBy,
            UpdatedDate = model.UpdatedDate,
            Channels = model.Channels,
            WorkgroupIds = model.WorkgroupIds,
            NotificationSubscriptionOverrides =NotificationSubscriptionOverrideDto.MapFrom(model.NotificationSubscriptionOverrides),
            PriorityIds = model.PriorityIds,
            SkillCategoryIds = model.SkillCategoryIds,
            TwinCategoryIds = model.TwinCategoryIds,
            SkillIds = model.SkillIds,
            Twins = NotificationTriggerTwinDto.MapFrom(model.Twins),
            IsMuted = model.IsMuted
        } :null;
    }

    public static IEnumerable<NotificationTriggerDto> MapFrom(IEnumerable<NotificationTrigger> models)
    {
        return models?.Select(MapFrom);
    }
}

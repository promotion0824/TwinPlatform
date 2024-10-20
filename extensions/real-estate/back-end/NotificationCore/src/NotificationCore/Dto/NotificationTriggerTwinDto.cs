using NotificationCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace NotificationCore.Dto;

public class NotificationTriggerTwinDto
{
    public string TwinId { get; set; }
    public string TwinName { get; set; }

    public static List<NotificationTriggerTwinDto> MapFrom(List<NotificationTriggerTwin> modelTwins)
    {
        return modelTwins?.Select(c => new NotificationTriggerTwinDto
        {
            TwinId = c.TwinId,
            TwinName = c.TwinName
        }).ToList();
    }

    public static List<NotificationTriggerTwin> MapTo(List<NotificationTriggerTwinDto> modelTwins)
    {
        return modelTwins?.Select(c => new NotificationTriggerTwin
        {
            TwinId = c.TwinId,
            TwinName = c.TwinName
        }).ToList();
    }
}

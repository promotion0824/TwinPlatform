using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Dto
{
    public class NotificationReceiverDto
    {
        public Guid UserId { get; set; }

        public static NotificationReceiverDto MapFromModel(NotificationReceiver model)
        {
            return new NotificationReceiverDto
            {
                UserId = model.UserId
            };
        }

        public static List<NotificationReceiverDto> MapFromModels(List<NotificationReceiver> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}

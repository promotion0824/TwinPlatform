using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_NotificationReceiver")]
    public class NotificationReceiverEntity
    {
        public Guid SiteId { get; set; }
        public Guid UserId { get; set; }

        public static NotificationReceiver MapToModel(NotificationReceiverEntity entity)
        {
            return new NotificationReceiver
            {
                SiteId = entity.SiteId,
                UserId = entity.UserId
            };
        }

        public static List<NotificationReceiver> MapToModels(IEnumerable<NotificationReceiverEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

    }
}

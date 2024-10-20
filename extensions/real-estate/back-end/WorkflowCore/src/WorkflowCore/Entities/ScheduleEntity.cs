using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Models;
using Willow.Scheduler;

namespace WorkflowCore.Entities
{
    [Table("Schedule")]
    public class ScheduleEntity
    {
        public Guid   Id              { get; set; }
        public bool   Active          { get; set; } = true;
        public Guid   OwnerId         { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Recurrence      { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string RecipientClient { get; set; } 

        [Required(AllowEmptyStrings = false)]
        public string Recipient       { get; set; } 

        public static ScheduleEntity MapFromModel(Schedule model)
        {
            return new ScheduleEntity
            {
                Id               = model.Id,             
                Active           = model.Active,
                OwnerId          = model.OwnerId,
                Recurrence       = model.Recurrence,
                RecipientClient  = model.RecipientClient,
                Recipient        = model.Recipient      
            };
        }

        public static List<ScheduleEntity> MapFromModels(IEnumerable<Schedule> models)
        {
            return models?.Select(MapFromModel).ToList();
        }

        public static Schedule MapToModel(ScheduleEntity entity)
        {
            return new Schedule
            {
                Id               = entity.Id,             
                Active           = entity.Active,
                OwnerId          = entity.OwnerId,
                Recurrence       = entity.Recurrence,
                RecipientClient  = entity.RecipientClient,
                Recipient        = entity.Recipient      
            };
        }

        public static List<Schedule> MapToModels(IEnumerable<ScheduleEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }
    }
}
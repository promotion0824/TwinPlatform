using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

using Willow.Scheduler;

namespace WorkflowCore.Dto
{
    public class ScheduleDto
    {
        public Guid   Id              { get; set; }
        public bool   Active          { get; set; } = true;
        public Guid   OwnerId         { get; set; }
        public string Recurrence      { get; set; }
        public string RecipientClient { get; set; } 
        public string Recipient       { get; set; } 

        public static ScheduleDto MapFromModel(Schedule model)
        {
            return new ScheduleDto
            {
                Id               = model.Id,
                Active           = model.Active,
                OwnerId          = model.OwnerId,
                Recurrence       = model.Recurrence,
                RecipientClient  = model.RecipientClient,
                Recipient        = model.Recipient      
            };
        }

        public static List<ScheduleDto> MapFromModels(List<Schedule> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}

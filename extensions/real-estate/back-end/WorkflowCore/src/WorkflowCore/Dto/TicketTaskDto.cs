using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Dto
{
    public class TicketTaskDto
    {
        public Guid Id { get; set; }
        public string TaskName { get; set; }
        public bool IsCompleted { get; set; }
        public int Order { get; set; }
        public double? NumberValue { get; set; }
        public TaskType Type { get; set; }
        public int? DecimalPlaces { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public string Unit { get; set; }

        public static TicketTaskDto MapFromModel(TicketTask model)
        {
            return new TicketTaskDto
            {
                Id = model.Id,
                TaskName = model.TaskName,
                IsCompleted = model.IsCompleted,
                Order = model.Order,
                NumberValue = model.NumberValue,
                Type = model.Type,
                DecimalPlaces = model.DecimalPlaces,
                MinValue = model.MinValue,
                MaxValue = model.MaxValue,
                Unit = model.Unit
            };
        }

        public static List<TicketTaskDto> MapFromModels(List<TicketTask> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}

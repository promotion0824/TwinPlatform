using System;
using System.Linq;
using System.Collections.Generic;
using MobileXL.Models;

namespace MobileXL.Dto
{
    public class TicketTaskDto
    {
        public Guid Id { get; set; }
        public string TaskName { get; set; }
        public bool IsCompleted { get; set; }
        public double? NumberValue { get; set; }
        public TicketTaskType Type { get; set; }
        public int? DecimalPlaces { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public string Unit { get; set; }

        public static TicketTaskDto MapFromModel(TicketTask ticketTask)
        {
            return new TicketTaskDto
            {
                Id = ticketTask.Id,
                TaskName = ticketTask.TaskName,
                IsCompleted = ticketTask.IsCompleted,
                NumberValue = ticketTask.NumberValue,
                Type = ticketTask.Type,
                DecimalPlaces = ticketTask.DecimalPlaces,
                MinValue = ticketTask.MinValue,
                MaxValue = ticketTask.MaxValue,
                Unit = ticketTask.Unit
            };
        }

        public static List<TicketTaskDto> MapFromModels(List<TicketTask> ticketTasks)
        {
            return ticketTasks?.Select(MapFromModel).ToList();
        }
    }
}

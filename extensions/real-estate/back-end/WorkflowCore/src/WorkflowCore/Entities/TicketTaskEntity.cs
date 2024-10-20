using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_TicketTask")]
    public class TicketTaskEntity
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set;}

        [Required(AllowEmptyStrings = false)]
        [MaxLength(300)]
        public string TaskName { get; set; }

        public bool IsCompleted { get; set; }
        public int Order { get; set; }
        public double? NumberValue { get; set; }
        public TaskType Type { get; set; }
        public int? DecimalPlaces { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public string Unit { get; set; }
        public TicketEntity Ticket { get; set; }

        public static TicketTaskEntity MapFromModel(TicketTask model)
        {
            return new TicketTaskEntity
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

        public static List<TicketTaskEntity> MapFromModels(IEnumerable<TicketTask> models)
        {
            return models?.Select(MapFromModel).ToList();
        }

        public static TicketTask MapToModel(TicketTaskEntity entity)
        {
            return new TicketTask
            {
                Id = entity.Id,
                TaskName = entity.TaskName,
                IsCompleted = entity.IsCompleted,
                Order = entity.Order,
                NumberValue = entity.NumberValue,
                Type = entity.Type,
                DecimalPlaces = entity.DecimalPlaces,
                MinValue = entity.MinValue,
                MaxValue = entity.MaxValue,
                Unit = entity.Unit
            };
        }

        public static List<TicketTask> MapToModels(IEnumerable<TicketTaskEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }
    }
}
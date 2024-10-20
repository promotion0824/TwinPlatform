using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_TicketStatus")]
    public class TicketStatusEntity
    {
        public Guid CustomerId { get; set; }
        public int StatusCode { get; set; }
        [MaxLength(64)]
        public string Status { get; set; }
        [MaxLength(16)]
        public string Tab { get; set; }
        [MaxLength(32)]
        public string Color { get; set; }

        public static TicketStatus MapToModel(TicketStatusEntity entity)
        {
            return new TicketStatus
            {
                CustomerId = entity.CustomerId,
                StatusCode = entity.StatusCode,
                Status = entity.Status,
                Tab = entity.Tab,
                Color = entity.Color
            };
        }

        public static List<TicketStatus> MapToModels(IEnumerable<TicketStatusEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static TicketStatusEntity MapFromModel(TicketStatus model)
        {
            return new TicketStatusEntity
            {
                CustomerId = model.CustomerId,
                StatusCode = model.StatusCode,
                Status = model.Status,
                Tab = model.Tab,
                Color = model.Color
            };
        }
    }
}

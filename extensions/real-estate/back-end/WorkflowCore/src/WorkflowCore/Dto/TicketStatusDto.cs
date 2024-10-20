using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Dto
{
    public class TicketStatusDto
    {
        public Guid CustomerId { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string Tab { get; set; }
        public string Color { get; set; }

        public static TicketStatusDto MapFromModel(TicketStatus model)
        {
            return new TicketStatusDto
            {
                CustomerId = model.CustomerId,
                StatusCode = model.StatusCode,
                Status = model.Status,
                Tab = model.Tab,
                Color = model.Color
            };
        }

        public static List<TicketStatusDto> MapFromModels(List<TicketStatus> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}

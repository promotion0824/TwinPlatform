using MobileXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileXL.Dto
{
    public class CustomerTicketStatusDto
    {
        public Guid CustomerId { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string Tab { get; set; }
        public string Color { get; set; }

        public static CustomerTicketStatusDto MapFromModel(CustomerTicketStatus model)
        {
            return new CustomerTicketStatusDto
            {
                CustomerId = model.CustomerId,
                StatusCode = model.StatusCode,
                Status = model.Status,
                Tab = model.Tab,
                Color = model.Color
            };
        }

        public static List<CustomerTicketStatusDto> MapFromModels(List<CustomerTicketStatus> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}

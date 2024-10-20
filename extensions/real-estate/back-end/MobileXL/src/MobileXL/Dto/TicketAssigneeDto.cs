using MobileXL.Models;
using System;

namespace MobileXL.Dto
{
    public class TicketAssigneeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public static TicketAssigneeDto MapFromModel(TicketAssignee assignee)
        {
            if (assignee == null)
                return null;

            return new TicketAssigneeDto
            {
                Id = assignee.Id,
                Name = assignee.Name
            };
        }
    }
}

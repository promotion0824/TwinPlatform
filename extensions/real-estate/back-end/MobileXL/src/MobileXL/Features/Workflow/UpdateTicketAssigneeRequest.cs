using System;
using MobileXL.Models;

namespace MobileXL.Features.Workflow
{
    public class UpdateTicketAssigneeRequest
    {
        public TicketAssigneeType AssigneeType { get; set; }
        public Guid? AssigneeId { get; set; }
    }
}

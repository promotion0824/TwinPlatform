using System;
using WorkflowCore.Models;

namespace WorkflowCore.Extensions
{
    public static class WorkflowExtensions
    {
        public static string ToTicketStatusString(this string ticketStatus)
        {
            return Enum.TryParse(ticketStatus, true, out TicketStatusEnum status) ? Enum.GetName(status) : ticketStatus;
        }
    }
}

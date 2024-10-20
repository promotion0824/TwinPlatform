using System;

namespace WorkflowCore.Models
{
    public class TicketStatus
    {
        public Guid CustomerId { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
        /// <summary>
        /// Tab can be used to filter ticket status in UI
        /// also used to categorize ticket status to Open, Resolved or Closed
        /// each ticket status, mapped to a Tab value
        /// e.g. to determine if ticket is closed we check all status under Closed tab 
        /// </summary>
        public string Tab { get; set; }
        public string Color { get; set; }
    }

    public enum TicketStatusEnum
    {
        Open = 0,
        Reassign = 5,
        InProgress = 10,
        LimitedAvailability = 15,
        Resolved = 20,
        Closed = 30,

        // Walmart additional  status
        New = 100,
        ReadyForWork = 110,
        RequestOnHold = 120,
        OnHold = 130,
        ClosedCompleted = 140,
        ClosedCancelled = 150,

        // Walmart retail additional status

        Void = 200,
        Completed = 210,
        Invoiced = 220

    }

    public static class TicketTabs
    {
        public const string OPEN = "Open";
        public const string RESOLVED = "Resolved";
        public const string CLOSED = "Closed";

    }
}

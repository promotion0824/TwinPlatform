using System;

namespace WorkflowCore.Models
{
    public class TicketCategory
    {
        public Guid Id { get; set; }
        public Guid? SiteId { get; set; }
        public string Name { get; set; }
    }
}

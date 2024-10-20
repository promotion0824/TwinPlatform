using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkflowCore.Entities
{
    [Table("WF_TicketInsights")]
    public class TicketInsightEntity
    {
        public Guid Id { get; set; }

        public Guid TicketId { get; set; }

        [ForeignKey(nameof(TicketId))]
        public TicketEntity Ticket { get; set; }

        public Guid InsightId { get; set; }

        public string InsightName { get; set; }
    }
}

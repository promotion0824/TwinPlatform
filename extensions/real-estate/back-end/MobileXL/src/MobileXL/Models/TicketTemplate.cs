using System;
using System.Collections.Generic;

namespace MobileXL.Models
{
    public class TicketTemplate
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SiteId { get; set; }
        public string FloorCode { get; set; }
        public string SequenceNumber { get; set; }
        public int Priority { get; set; }
        public TicketStatus Status { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public Guid? ReporterId { get; set; }
        public string ReporterName { get; set; }
        public string ReporterPhone { get; set; }
        public string ReporterEmail { get; set; }
        public string ReporterCompany { get; set; }
        public TicketAssigneeType AssigneeType { get; set; }
        public Guid? AssigneeId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public TicketSourceType SourceType { get; set; }
        public Event Recurrence { get; set; }
        public Duration OverdueThreshold { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Attachment> Attachments { get; set; }
        public List<TicketAsset> Assets { get; set; }
        public List<TicketTask> Tasks { get; set; }
    }
}

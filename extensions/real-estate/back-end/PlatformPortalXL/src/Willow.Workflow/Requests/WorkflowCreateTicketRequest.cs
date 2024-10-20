using System;
using System.Collections.Generic;

namespace Willow.Workflow
{
    public class WorkflowCreateTicketRequest
    {
        public Guid CustomerId { get; set; }
        public string FloorCode { get; set; }
        public string SequenceNumberPrefix { get; set; }
        public int Priority { get; set; }
        public TicketIssueType IssueType { get; set; }
        public Guid? IssueId { get; set; }
        public string IssueName { get; set; }
        public Guid? InsightId { get; set; }
        public string InsightName { get; set; }
        public List<CreateTicketRequestInsight> Diagnostics { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Cause { get; set; }
        public Guid? ReporterId { get; set; }
        public string ReporterName { get; set; }
        public string ReporterPhone { get; set; }
        public string ReporterEmail { get; set; }
        public string ReporterCompany { get; set; }
        public TicketAssigneeType AssigneeType { get; set; }
        public Guid? AssigneeId { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime? DueDate { get; set; }
        public TicketSourceType SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public string ExternalId { get; set; }
        public string ExternalStatus { get; set; }
        public string ExternalMetadata { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string TwinId { get; set; }
        public Guid? JobTypeId { get; set; }
        public Guid? ServiceNeededId { get; set; }
    }
}

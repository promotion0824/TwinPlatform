using System;
using System.Collections.Generic;

namespace Willow.Workflow
{
    public class WorkflowUpdateTicketRequest
    {
        public Guid CustomerId { get; set; }
        public int? Priority { get; set; }
        public int? Status { get; set; }
        public string FloorCode { get; set; }
        public TicketIssueType? IssueType { get; set; }
        public Guid? IssueId { get; set; }
        public string IssueName { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Cause { get; set; }
        public string Solution { get; set; }
        public bool ShouldUpdateReporterId { get; set; }
        public Guid? ReporterId { get; set; }
        public string ReporterName { get; set; }
        public string ReporterPhone { get; set; }
        public string ReporterEmail { get; set; }
        public string ReporterCompany { get; set; }
        public TicketAssigneeType? AssigneeType { get; set; }
        public Guid? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ExternalCreatedDate { get; set; }
        public DateTime? ExternalUpdatedDate { get; set; }
        public bool LastUpdatedByExternalSource { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int Occurrence { get; set; }
        public List<TicketAsset> Assets { get; set; }
        public List<TicketTask> Tasks { get; set; }
		public TicketSourceType SourceType { get; set; }
		public Guid SourceId { get; set; }
        public Guid? JobTypeId { get; set; }      
        public Guid? ServiceNeededId { get; set; }
        public Guid? SubStatusId { get; set; }
        public string TwinId { get; set; }
    }
}

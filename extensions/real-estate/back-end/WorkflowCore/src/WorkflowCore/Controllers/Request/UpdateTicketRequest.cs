using System;
using System.Collections.Generic;
using WorkflowCore.Models;

using Willow.Calendar;
using WorkflowCore.Controllers.Request.Validation;
using System.ComponentModel.DataAnnotations;

namespace WorkflowCore.Controllers.Request
{
    public class UpdateTicketRequest
    {
        public Guid CustomerId { get; set; }
        public int? Priority { get; set; }
        public int? Status { get; set; }
        public string FloorCode { get; set; }
        public IssueType? IssueType { get; set; }
        public Guid? IssueId { get; set; }
        public string IssueName { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Cause { get; set; }
        public string Solution { get; set; }
        public bool ShouldUpdateReporterId { get; set; }
        public Guid? ReporterId { get; set; }
        public string ReporterName { get; set; }
        public string ReporterPhone { get; set; }
        public string ReporterEmail { get; set; }
        public string ReporterCompany { get; set; }
        [RequiredAssigneeType("AssigneeId")]
        public AssigneeType? AssigneeType { get; set; }
        public Guid? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ExternalCreatedDate { get; set; }
        public DateTime? ExternalUpdatedDate { get; set; }
        public bool LastUpdatedByExternalSource { get; set; }
        public string ExternalMetadata { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }
        public List<string> ExtendableSearchablePropertyKeys { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public Guid? CategoryId { get; set; }
        public Event Recurrence { get; set; }
        public string Notes { get; set; }
        public List<TicketTask> Tasks { get; set; }

        public SourceType? SourceType { get; set; }
        public Guid? SourceId { get; set; }

        [StringLength(250)]
        public string TwinId { get; set; }
        public Guid? SubStatusId { get; set; }

        [StringLength(250)]
        public string SpaceTwinId { get; set; }
        public Guid? JobTypeId { get; set; }
        public Guid? ServiceNeededId { get; set; }
    }
}

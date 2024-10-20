using System;
using System.Collections.Generic;
using WorkflowCore.Controllers.Request.Validation;
using WorkflowCore.Models;

namespace WorkflowCore.Controllers.Request
{
    public class CreateTicketsRequest
    {
        public class CreateTicketsRequestItem
        {
            public int Status { get; set; }
            public string FloorCode { get; set; }
            public int Priority { get; set; }
            public IssueType IssueType { get; set; }
            public Guid? IssueId { get; set; }
            public string IssueName { get; set; }
            public string Summary { get; set; }
            public string Description { get; set; }
            public string Cause { get; set; }
            public string ReporterName { get; set; }
            public string ReporterPhone { get; set; }
            public string ReporterEmail { get; set; }
            public string ReporterCompany { get; set; }
            [RequiredAssigneeType("AssigneeId")]
            public AssigneeType AssigneeType { get; set; }
            public Guid? AssigneeId { get; set; }
            public Guid CreatorId { get; set; }
            public DateTime? DueDate { get; set; }
            public string ExternalId { get; set; }
            public string ExternalStatus { get; set; }
            public string ExternalMetadata { get; set; }
            public Dictionary<string, string> CustomProperties { get; set; }
            public List<string> ExtendableSearchablePropertyKeys { get; set; }
            public DateTime? ExternalCreatedDate { get; set; }
            public DateTime? ExternalUpdatedDate { get; set; }
            public bool LastUpdatedByExternalSource { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public Guid? CategoryId { get; set; }
            public string TwinId { get; set; }
        }

        public Guid CustomerId { get; set; }
        public string SequenceNumberPrefix { get; set; }
        public SourceType SourceType { get; set; }
        public Guid? SourceId { get; set; }

        public List<CreateTicketsRequestItem> Tickets { get; set; }
    }
}

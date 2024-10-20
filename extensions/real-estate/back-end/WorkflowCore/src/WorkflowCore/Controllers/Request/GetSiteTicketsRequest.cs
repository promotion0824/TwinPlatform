using System;
using System.Collections.Generic;
using WorkflowCore.Models;

namespace WorkflowCore.Controllers.Request
{
    public class GetSiteTicketsRequest
    {
        public IList<int> Statuses { get; set; }
        public IssueType? IssueType { get; set; }
        public Guid? IssueId { get; set; }
        public Guid? InsightId { get; set; }
        public Guid? AssigneeId { get; set; }
        public bool? Unassigned { get; set; }
        public bool IsScheduled { get; set; }
        public string OrderBy { get; set; }
        public string ExternalId { get; set; }
        public string FloorId { get; set; }        
        public Guid? SourceId { get; set; }
        public SourceType? SourceType { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}

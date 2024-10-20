using System;
using System.Collections.Generic;

namespace MobileXL.Models
{
	public class Ticket
	{
		public Guid Id { get; set; }
		public Guid CustomerId { get; set; }
		public Guid SiteId { get; set; }
		public string FloorCode { get; set; }
		public string SequenceNumber { get; set; }
		public int Priority { get; set; }
		public int Status { get; set; }
		public TicketIssueType IssueType { get; set; }
		public Guid? IssueId { get; set; }
		public string IssueName { get; set; }
		public Guid? InsightId { get; set; }
		public string InsightName { get; set; }
		public string Summary { get; set; }
		public string Description { get; set; }
		public string Cause { get; set; }
		public string Solution { get; set; }
		public Guid? ReporterId { get; set; }
		public string ReporterName { get; set; }
		public string ReporterPhone { get; set; }
		public string ReporterEmail { get; set; }
		public string ReporterCompany { get; set; }
		public TicketAssigneeType AssigneeType { get; set; }
		public Guid? AssigneeId { get; set; }
		public Guid CreatorId { get; set; }
		public DateTime? DueDate { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime UpdatedDate { get; set; }
		public DateTime? ExternalCreatedDate { get; set; }
		public DateTime? ExternalUpdatedDate { get; set; }
		public bool LastUpdatedByExternalSource { get; set; }
		public DateTime? ResolvedDate { get; set; }
		public DateTime? ClosedDate { get; set; }
		public TicketSourceType SourceType { get; set; }
		public Guid? SourceId { get; set; }
		public string ExternalId { get; set; }
		public string ExternalStatus { get; set; }
		public string ExternalMetadata { get; set; }
		public Guid? CategoryId { get; set; }
		public string Category { get; set; }
		public int Occurrence { get; set; }
		public Guid? TemplateId { get; set; }
		public string Notes { get; set; }
		public List<Comment> Comments { get; set; }
		public List<Attachment> Attachments { get; set; }
		public List<TicketAsset> Assets { get; set; }
		public List<TicketTask> Tasks { get; set; }
		public TicketAssignee Assignee { get; set; }
	}
}

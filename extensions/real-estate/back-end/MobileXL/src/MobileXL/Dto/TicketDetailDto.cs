using System;
using System.Collections.Generic;
using MobileXL.Models;
using MobileXL.Services;

namespace MobileXL.Dto
{
	public class TicketDetailDto
	{
		public Guid Id { get; set; }
		public Guid CustomerId { get; set; }
		public Guid SiteId { get; set; }
		public string FloorCode { get; set; }
		public string SequenceNumber { get; set; }
		public int Priority { get; set; }
		public int StatusCode { get; set; }
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
		public DateTime? DueDate { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime UpdatedDate { get; set; }
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
		public string Notes { get; set; }
		public List<CommentDto> Comments { get; set; }
		public List<AttachmentDto> Attachments { get; set; }
		public List<TicketAssetDto> Assets { get; set; }
		public List<TicketTaskDto> Tasks { get; set; }
		public TicketAssigneeDto Assignee { get; set; }


		public static TicketDetailDto MapFromModel(Ticket model, IImageUrlHelper helper)
		{
			return new TicketDetailDto
			{
				Id = model.Id,
				CustomerId = model.CustomerId,
				SiteId = model.SiteId,
				FloorCode = model.FloorCode,
				SequenceNumber = model.SequenceNumber,
				Priority = model.Priority,
				StatusCode = model.Status,
				IssueType = model.IssueType,
				IssueId = model.IssueId,
				IssueName = model.IssueName,
				InsightId = model.InsightId,
				InsightName = model.InsightName,
				Summary = model.Summary,
				Description = model.Description,
				Cause = model.Cause,
				Solution = model.Solution,
				ReporterId = model.ReporterId,
				ReporterName = model.ReporterName,
				ReporterPhone = model.ReporterPhone,
				ReporterEmail = model.ReporterEmail,
				ReporterCompany = model.ReporterCompany,
				AssigneeType = model.AssigneeType,
				AssigneeId = model.AssigneeId,
				DueDate = model.DueDate,
				CreatedDate = model.LastUpdatedByExternalSource ? model.ExternalCreatedDate ?? model.CreatedDate : model.CreatedDate,
				UpdatedDate = model.LastUpdatedByExternalSource ? model.ExternalUpdatedDate ?? model.UpdatedDate : model.UpdatedDate,
				ResolvedDate = model.ResolvedDate,
				ClosedDate = model.ClosedDate,
				SourceType = model.SourceType,
				SourceId = model.SourceId,
				ExternalId = model.ExternalId,
				ExternalStatus = model.ExternalStatus,
				ExternalMetadata = model.ExternalMetadata,
				CategoryId = model.CategoryId,
				Category = model.Category,
				Occurrence = model.Occurrence,
				Notes = model.Notes,
				Comments = CommentDto.MapFromModels(model.Comments),
				Attachments = AttachmentDto.MapFromModels(model.Attachments, helper),
				Assets = TicketAssetDto.MapFromModels(model.Assets),
				Tasks = TicketTaskDto.MapFromModels(model.Tasks),
				Assignee = TicketAssigneeDto.MapFromModel(model.Assignee)
			};
		}
	}
}

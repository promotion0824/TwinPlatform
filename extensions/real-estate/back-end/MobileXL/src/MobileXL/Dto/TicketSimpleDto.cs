using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;

namespace MobileXL.Dto
{
	public class TicketSimpleDto
	{
		public Guid Id { get; set; }
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
		public string ReporterName { get; set; }
		public TicketAssigneeType AssigneeType { get; set; }
		public Guid? AssigneeId { get; set; }
		public DateTime? DueDate { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime UpdatedDate { get; set; }
		public DateTime? ResolvedDate { get; set; }
		public DateTime? ClosedDate { get; set; }
		public Guid? CategoryId { get; set; }
		public string Category { get; set; }
		public List<TicketTaskDto> Tasks { get; set; }

		public static TicketSimpleDto MapFromModel(Ticket model)
		{
			return new TicketSimpleDto
			{
				Id = model.Id,
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
				ReporterName = model.ReporterName,
				AssigneeType = model.AssigneeType,
				AssigneeId = model.AssigneeId,
				DueDate = model.DueDate,
				CreatedDate = model.LastUpdatedByExternalSource ? model.ExternalCreatedDate ?? model.CreatedDate : model.CreatedDate,
				UpdatedDate = model.LastUpdatedByExternalSource ? model.ExternalUpdatedDate ?? model.UpdatedDate : model.UpdatedDate,
				ResolvedDate = model.ResolvedDate,
				ClosedDate = model.ClosedDate,
				CategoryId = model.CategoryId,
				Category = model.Category,
				Tasks = TicketTaskDto.MapFromModels(model.Tasks)
			};
		}

		public static List<TicketSimpleDto> MapFromModels(List<Ticket> models)
		{
			return models?.Select(MapFromModel).ToList();
		}

	}
}

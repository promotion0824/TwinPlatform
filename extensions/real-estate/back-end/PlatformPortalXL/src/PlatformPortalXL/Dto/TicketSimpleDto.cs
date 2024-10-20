using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class TicketSimpleDto : Assignable
    {
        public Guid Id { get; set; }
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
        public string ReporterName { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public Guid? CategoryId { get; set; }
        public string Category { get; set; }
        public Guid? SourceId { get; set; }
        public string SourceName { get; set; }
        public string ExternalId { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public List<TicketTaskDto> Tasks { get; set; }
        public int GroupTotal { get; set; }
        public string TwinId { get; set; }
		public static TicketSimpleDto MapFromModel(Ticket model)
        {
            return new TicketSimpleDto
            {
                Id = model.Id,
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
                ReporterName = model.ReporterName,
                AssigneeType = model.AssigneeType,
                AssigneeId = model.AssigneeId,
                AssignedTo = model.AssigneeName,
                DueDate = model.DueDate,
                CreatedDate = model.ComputedCreatedDate, 
                UpdatedDate = model.ComputedUpdatedDate, 
                ResolvedDate = model.ResolvedDate,
                ClosedDate = model.ClosedDate,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                CategoryId = model.CategoryId,
                Category = model.Category,
                SourceId = model.SourceId,
                SourceName = model.SourceName,
                ExternalId = model.ExternalId,
                ScheduledDate = model.ScheduledDate,
				TwinId = model.TwinId,
                Tasks = TicketTaskDto.MapFromModels(model.Tasks)
            };
        }

        public static List<TicketSimpleDto> MapFromModels(List<Ticket> models)
        {
            return models?.Select(MapFromModel).ToList();
        }

    }
}

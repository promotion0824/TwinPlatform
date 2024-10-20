using System;
using System.Collections.Generic;

using PlatformPortalXL.Services;
using Willow.Workflow;

namespace PlatformPortalXL.Dto
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
        public List<TicketInsight> Diagnostics { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Cause { get; set; }
        public string Solution { get; set; }
        public string Notes { get; set; }
        public Guid? ReporterId { get; set; }
        public string ReporterName { get; set; }
        public string ReporterPhone { get; set; }
        public string ReporterEmail { get; set; }
        public string ReporterCompany { get; set; }
        public TicketAssigneeType AssigneeType { get; set; }
        public Guid? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public TicketSourceType SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public string SourceName { get; set; }
        public string ExternalId { get; set; }
        public string ExternalStatus { get; set; }
        public string ExternalMetadata { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public Guid? CategoryId { get; set; }
        public string Category { get; set; }
        public int Occurrence { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public bool Template { get; set; }
        public TicketAssignee Assignee { get; set; }
        public TicketCreator Creator { get; set; }
		public List<CommentDto> Comments { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
        public List<TicketTaskDto> Tasks { get; set; }
		public string TwinId { get; set; }
		/// <summary>
		/// Check if this ticket is the last not closed ticket for the insight associated with it
		/// </summary>
		public bool? CanResolveInsight { get; set; }
        /// <summary>
        /// Ticket sub status
        /// </summary>
        public Guid? SubStatusId { get; set; }
        /// <summary>
        /// Ticket space twin id
        /// </summary>
        public string SpaceTwinId { get; set; }
        /// <summary>
        /// Ticket job type id
        /// </summary>
        public Guid? JobTypeId { get; set; }
        /// <summary>
        /// Ticket service needed id
        /// </summary>
        public Guid? ServiceNeededId { get; set; }

        /// <summary>
        /// Valid ticket status for transition from the current status
        /// This feature only available for Mapped enabled customers
        /// </summary>
        public List<int> NextValidStatus { get; set; }
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
                Diagnostics = model.Diagnostics,
                Summary = model.Summary,
                Description = model.Description,
                Cause = model.Cause,
                Solution = model.Solution,
                Notes = model.Notes,
                ReporterId = model.ReporterId,
                ReporterName = model.ReporterName,
                ReporterPhone = model.ReporterPhone,
                ReporterEmail = model.ReporterEmail,
                ReporterCompany = model.ReporterCompany,
                AssigneeType = model.AssigneeType,
                AssigneeId = model.AssigneeId,
                DueDate = model.DueDate,
                CreatedDate = model.ComputedCreatedDate, 
                UpdatedDate = model.ComputedUpdatedDate,
                ResolvedDate = model.ResolvedDate,
                ClosedDate = model.ClosedDate,
                SourceType = model.SourceType,
                SourceId = model.SourceId,
                SourceName = model.SourceName,
                ExternalId = model.ExternalId,
                ExternalStatus = model.ExternalStatus,
                ExternalMetadata = model.ExternalMetadata,
                Latitude = model.Latitude, 
                Longitude = model.Longitude,
                CategoryId = model.CategoryId,
                Category = model.Category,
                Assignee = model.Assignee,
				Creator = model.Creator,
                ScheduledDate = model.ScheduledDate,
                Template = model.TemplateId.HasValue,
                Comments = CommentDto.MapFromModels(model.Comments),
                Attachments = AttachmentDto.MapFromModels(model.Attachments, helper),
                Tasks = TicketTaskDto.MapFromModels(model.Tasks),
				TwinId = model.TwinId,
				CanResolveInsight = model.CanResolveInsight,
                SubStatusId = model.SubStatusId,
                SpaceTwinId = model.SpaceTwinId,
                JobTypeId = model.JobTypeId,
                ServiceNeededId = model.ServiceNeededId,
                NextValidStatus = model.NextValidStatus
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;
using WorkflowCore.Services.Apis;

namespace WorkflowCore.Dto
{
    public class TicketDetailDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SiteId { get; set; }
        public string FloorCode { get; set; }
        public string SequenceNumber { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public IssueType IssueType { get; set; }
        public Guid? IssueId { get; set; }
        public string IssueName { get; set; }
        public Guid? InsightId { get; set; }
        public string InsightName { get; set; }
        public List<Insight> Diagnostics { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Cause { get; set; }
        public string Solution { get; set; }
        public Guid? ReporterId { get; set; }
        public string ReporterName { get; set; }
        public string ReporterPhone { get; set; }
        public string ReporterEmail { get; set; }
        public string ReporterCompany { get; set; }
        public AssigneeType AssigneeType { get; set; }
        public Guid? AssigneeId { get; set; }
        public string AssigneeName { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public SourceType SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public string SourceName { get; set; }
        public string ExternalId { get; set; }
        public string ExternalStatus { get; set; }
        public string ExternalMetadata { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }
        public List<string> ExtendableSearchablePropertyKeys { get; set; }
        public DateTime? ExternalCreatedDate { get; set; }
        public DateTime? ExternalUpdatedDate { get; set; }
        public bool LastUpdatedByExternalSource { get; set; }
        public DateTime ComputedCreatedDate { get; set; }
        public DateTime ComputedUpdatedDate { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Category { get; set; }
        public Guid? CategoryId { get; set; }

        public List<CommentDto> Comments { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
        public List<TicketTaskDto> Tasks { get; set; }
        
        // Scheduled ticket
        public DateTime?        ScheduledDate   { get; set; }
        public int              Occurrence      { get; set; } = 0;
        public Guid?            TemplateId      { get; set; }
        public string           Notes           { get; set; }
        public string TwinId { get; set; }
        /// <summary>
        /// Check if this ticket is the last not closed ticket for the insight associated with it
        /// </summary>
        public bool? CanResolveInsight { get; set; }

        /// <summary>
        /// The sub status of the ticket
        /// </summary>
        public Guid? SubStatusId { get; set; }
        /// <summary>
        /// the job type Id of the ticket
        /// </summary>
        public Guid? JobTypeId { get; set; }
        /// <summary>
        /// The space twin id of the ticket e.g. the space twin id of the room/floor where the ticket was created
        /// </summary>
        public string SpaceTwinId { get; set; }

        /// <summary>
        /// The service needed id of the ticket
        /// </summary>
        public Guid? ServiceNeededId { get; set; }

        /// <summary>
        /// Valid ticket status for transition from the current status
        /// This feature only available for Mapped enabled customers
        /// </summary>
        public List<int> NextValidStatus { get; set; }

        public static TicketDetailDto MapFromModel(Ticket model, IImagePathHelper helper, string sourceName = null)
        {
            return new TicketDetailDto
            {
                Id = model.Id,
                CustomerId = model.CustomerId,
                SiteId = model.SiteId,
                FloorCode = model.FloorCode,
                SequenceNumber = model.SequenceNumber,
                Priority = model.Priority,
                Status = model.Status,
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
                ReporterId = model.ReporterId,
                ReporterName = model.ReporterName,
                ReporterPhone = model.ReporterPhone,
                ReporterEmail = model.ReporterEmail,
                ReporterCompany = model.ReporterCompany,
                AssigneeType = model.AssigneeType,
                AssigneeId = model.AssigneeId,
                AssigneeName = model.AssigneeName,
                CreatorId = model.CreatorId,
                DueDate = model.DueDate,
                CreatedDate = model.CreatedDate,
                UpdatedDate = model.UpdatedDate,
                StartedDate = model.StartedDate,
                ResolvedDate = model.ResolvedDate,
                ClosedDate = model.ClosedDate,
                SourceType = model.SourceType,
                SourceId = model.SourceId,
                SourceName = model.SourceType == SourceType.Mapped && !string.IsNullOrWhiteSpace(sourceName) ? sourceName : model.SourceName,
                ExternalId = model.ExternalId,
                ExternalStatus = model.ExternalStatus,
                ExternalMetadata = model.ExternalMetadata,
                CustomProperties = model.CustomProperties,
                ExtendableSearchablePropertyKeys = model.ExtendableSearchablePropertyKeys,
                ExternalCreatedDate = model.ExternalCreatedDate,
                ExternalUpdatedDate = model.ExternalUpdatedDate,
                LastUpdatedByExternalSource = model.LastUpdatedByExternalSource,
                ComputedCreatedDate = model.ComputedCreatedDate,
                ComputedUpdatedDate = model.ComputedUpdatedDate,
                Latitude = model.Latitude, 
                Longitude = model.Longitude,
                CategoryId = model.CategoryId,
                Category = model.Category == null ? "Unspecified" : model.Category.Name,
                TwinId = model.TwinId,
                Comments = CommentDto.MapFromModels(model.Comments),
                Attachments = AttachmentDto.MapFromTicketModels(model.Attachments, helper, model),
                Tasks = TicketTaskDto.MapFromModels(model.Tasks),
                
                ScheduledDate = model.ScheduledDate,
                Occurrence    = model.Occurrence,
                TemplateId    = model.TemplateId,
                Notes         = model.Notes,
				CanResolveInsight = model.CanResolveInsight,
                SubStatusId = model.SubStatusId,
                JobTypeId = model.JobTypeId,
                SpaceTwinId = model.SpaceTwinId,
                ServiceNeededId = model.ServiceNeededId,
                NextValidStatus = model.NextValidStatus
                
            };
        }

        public static List<TicketDetailDto> MapFromModels(List<Ticket> tickets, IImagePathHelper helper)
        {
            return tickets.Select(c => MapFromModel(c,helper)).ToList();
        }
    }
}

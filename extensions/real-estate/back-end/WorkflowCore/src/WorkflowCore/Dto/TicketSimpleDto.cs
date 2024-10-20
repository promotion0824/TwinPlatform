using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Dto
{
    public class TicketSimpleDto
    {
        public Guid Id { get; set; }
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
        public string Summary { get; set; }
        public string Description { get; set; }
        public Guid? ReporterId { get; set; }
        public string ReporterName { get; set; }
        public string ReporterPhone { get; set; }
        public string ReporterEmail { get; set; }
        public string ReporterCompany { get; set; }
        public AssigneeType AssigneeType { get; set; }
        public Guid? AssigneeId { get; set; }
        public string AssigneeName { get; set; }
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
        public DateTime? ExternalCreatedDate { get; set; }
        public DateTime? ExternalUpdatedDate { get; set; }
        public bool LastUpdatedByExternalSource { get; set; }
        public DateTime ComputedCreatedDate { get; set; }
        public DateTime ComputedUpdatedDate { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public Guid? CategoryId { get; set; }
        public string Category { get; set; }
        public string TwinId { get; set; }
        public string SpaceTwinId { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }
        public List<string> ExtendableSearchablePropertyKeys { get; set; }
        // Scheduled ticket
        public DateTime?           ScheduledDate   { get; set; }
        public int                 Occurrence      { get; set; } = 0;
        public Guid?               TemplateId      { get; set; }
        public List<TicketTaskDto> Tasks           { get; set; } 

        public static TicketSimpleDto MapFromModel(Ticket model, string sourceName = null)
        {
            return new TicketSimpleDto
            {
                Id = model.Id,
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
                Summary = model.Summary,
                Description = model.Description,
                ReporterId = model.ReporterId,
                ReporterName = model.ReporterName,
                ReporterPhone = model.ReporterPhone,
                ReporterEmail = model.ReporterEmail,
                ReporterCompany = model.ReporterCompany,
                AssigneeType = model.AssigneeType,
                AssigneeId = model.AssigneeId,
                AssigneeName = model.AssigneeName,
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
                SpaceTwinId = model.SpaceTwinId,
                CustomProperties = model.CustomProperties,
                ExtendableSearchablePropertyKeys = model.ExtendableSearchablePropertyKeys,
                ScheduledDate = model.ScheduledDate,
                Occurrence    = model.Occurrence,
                TemplateId    = model.TemplateId,
                Tasks         = TicketTaskDto.MapFromModels(model.Tasks)
            };
        }

        public static List<TicketSimpleDto> MapFromModels(List<Ticket> models, string sourceName = null)
        {
            return models?.Select(x => MapFromModel(x, sourceName)).ToList();
        }

    }
}

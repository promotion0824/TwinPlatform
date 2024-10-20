using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Infrastructure.Json;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
	[Table("WF_Ticket")]
    public class TicketEntity : IAuditTrail
	{
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }

        public Guid SiteId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(64)]
        public string FloorCode { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(64)]
        public string SequenceNumber { get; set; }

        public int Priority { get; set; }

        public int Status { get; set; }

        public IssueType IssueType { get; set; }

        public Guid? IssueId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(450)]
        public string IssueName { get; set; }

        public Guid? InsightId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(128)]
        public string InsightName { get; set; }

        [InverseProperty(nameof(TicketInsightEntity.Ticket))]
        public List<TicketInsightEntity> Diagnostics { get; set; } = new List<TicketInsightEntity>();

        [Required(AllowEmptyStrings = true)]
		[MaxLength(512)]
		public string Summary { get; set; }

        [Required(AllowEmptyStrings = true)]
		public string Description { get; set; }

        [Required(AllowEmptyStrings = true)]
		[MaxLength(1024)]
		public string Cause { get; set; }

        [Required(AllowEmptyStrings = true)]
		[MaxLength(1024)]
		public string Solution { get; set; }

        public Guid? ReporterId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(500)]
        public string ReporterName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(32)]
        public string ReporterPhone { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(64)]
        public string ReporterEmail { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(64)]
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

        [Required(AllowEmptyStrings = true)]
        [MaxLength(128)]
        public string ExternalId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(64)]
        public string ExternalStatus { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string ExternalMetadata { get; set; }

        public string CustomProperties { get; set; }

        public string ExtendableSearchablePropertyKeys { get; set; }

        public DateTime? ExternalCreatedDate { get; set; }

        public DateTime? ExternalUpdatedDate { get; set; }

        public bool LastUpdatedByExternalSource { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ComputedCreatedDate { get { return LastUpdatedByExternalSource ? ExternalCreatedDate ?? CreatedDate : CreatedDate; } private set { } }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ComputedUpdatedDate { get { return LastUpdatedByExternalSource ? ExternalUpdatedDate ?? UpdatedDate : UpdatedDate; } private set { } }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public TicketCategoryEntity Category { get; set; }

        public Guid? CategoryId { get; set; }

        public DateTime? ScheduledDate   { get; set; }
        public int Occurrence { get; set; } = 0; // Daydex of schedule hit
        public Guid? TemplateId { get; set; }

        [Required(AllowEmptyStrings = true)]
		[MaxLength(1000)]
		public string Notes { get; set; }
        [MaxLength(250)]
        public string TwinId { get; set; }

        public Guid? JobTypeId { get; set; }
        [ForeignKey(nameof(JobTypeId))]
        public JobTypeEntity JobType { get; set; }

        [MaxLength(250)]
        public string SpaceTwinId { get; set; }
        public List<CommentEntity> Comments { get; set; }
        public List<AttachmentEntity> Attachments { get; set; }
        public List<TicketTaskEntity> Tasks { get; set; }

        public Guid? ServiceNeededId { get; set; }

        [ForeignKey(nameof(ServiceNeededId))]
        public ServiceNeededEntity ServiceNeeded { get; set; }

        public Guid? SubStatusId { get; set; }

        [ForeignKey(nameof(SubStatusId))]
        public TicketSubStatusEntity SubStatus { get; set; }

        // This is a transient property that is not mapped to the database.
        // It is used to indicate that the ticket entity is new or updated in efcore interceptor
        [NotMapped]
        public EntityState? EntityLifeCycleState { get; set; }

        public static TicketEntity MapFromModel(Ticket ticketModel)
        {
            return new TicketEntity
            {
                Id = ticketModel.Id,
                CustomerId = ticketModel.CustomerId,
                SiteId = ticketModel.SiteId,
                FloorCode = ticketModel.FloorCode,
                SequenceNumber = ticketModel.SequenceNumber,
                Priority = ticketModel.Priority,
                Status = ticketModel.Status,
                IssueType = ticketModel.IssueType,
                IssueId = ticketModel.IssueId,
                IssueName = ticketModel.IssueName,
                InsightId = ticketModel.InsightId,
                InsightName = ticketModel.InsightName ?? string.Empty,
                Summary = ticketModel.Summary,
                Description = ticketModel.Description,
                Cause = ticketModel.Cause,
                Solution = ticketModel.Solution,
                ReporterId = ticketModel.ReporterId,
                ReporterName = ticketModel.ReporterName,
                ReporterPhone = ticketModel.ReporterPhone,
                ReporterEmail = ticketModel.ReporterEmail,
                ReporterCompany = ticketModel.ReporterCompany,
                AssigneeType = ticketModel.AssigneeType,
                AssigneeId = ticketModel.AssigneeId,
                AssigneeName = ticketModel.AssigneeName,
                CreatorId = ticketModel.CreatorId,
                DueDate = ticketModel.DueDate,
                CreatedDate = ticketModel.CreatedDate,
                UpdatedDate = ticketModel.UpdatedDate,
                ResolvedDate = ticketModel.ResolvedDate,
                ClosedDate = ticketModel.ClosedDate,
                SourceType = ticketModel.SourceType,
                SourceId = ticketModel.SourceId,
                SourceName = ticketModel.SourceName,
                ExternalId = ticketModel.ExternalId,
                ExternalStatus = ticketModel.ExternalStatus,
                ExternalMetadata = ticketModel.ExternalMetadata,
                CustomProperties = JsonConvert.SerializeObject(ticketModel.CustomProperties, JsonSettings.CaseSensitive),
                ExtendableSearchablePropertyKeys = JsonConvert.SerializeObject(ticketModel.ExtendableSearchablePropertyKeys, JsonSettings.CaseSensitive),
                ExternalCreatedDate = ticketModel.ExternalCreatedDate,
                ExternalUpdatedDate = ticketModel.ExternalUpdatedDate,
                LastUpdatedByExternalSource = ticketModel.LastUpdatedByExternalSource,
                Latitude = ticketModel.Latitude,
                Longitude = ticketModel.Longitude,
                Category = TicketCategoryEntity.MapFromModel(ticketModel.Category),
                CategoryId = ticketModel.CategoryId,
                Tasks = TicketTaskEntity.MapFromModels(ticketModel.Tasks),
                Notes = ticketModel.Notes,
                Occurrence = ticketModel.Occurrence,
                TemplateId = ticketModel.TemplateId,
                ScheduledDate = ticketModel.ScheduledDate,
                TwinId = ticketModel.TwinId,
                SubStatusId = ticketModel.SubStatusId,
                JobTypeId = ticketModel.JobTypeId,
                SpaceTwinId = ticketModel.SpaceTwinId,
                ServiceNeededId = ticketModel.ServiceNeededId,
                Diagnostics = ticketModel.Diagnostics?.Select(x => new TicketInsightEntity { InsightId = x.Id, InsightName = x.Name }).ToList()
            };
        }

        public static Ticket MapToModel(TicketEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new Ticket
            {
                Id = entity.Id,
                CustomerId = entity.CustomerId,
                SiteId = entity.SiteId,
                FloorCode = entity.FloorCode,
                SequenceNumber = entity.SequenceNumber,
                Priority = entity.Priority,
                Status = entity.Status,
                IssueType = entity.IssueType,
                IssueId = entity.IssueId,
                IssueName = entity.IssueName,
                InsightId = entity.InsightId,
                InsightName = entity.InsightName,
                Summary = entity.Summary,
                Description = entity.Description,
                Cause = entity.Cause,
                Solution = entity.Solution,
                ReporterId = entity.ReporterId,
                ReporterName = entity.ReporterName,
                ReporterPhone = entity.ReporterPhone,
                ReporterEmail = entity.ReporterEmail,
                ReporterCompany = entity.ReporterCompany,
                AssigneeType = entity.AssigneeType,
                AssigneeId = entity.AssigneeId,
                AssigneeName = entity.AssigneeName ?? "Unassigned",
                CreatorId = entity.CreatorId,
                DueDate = entity.DueDate,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                StartedDate = entity.StartedDate,
                ResolvedDate = entity.ResolvedDate,
                ClosedDate = entity.ClosedDate,
                SourceType = entity.SourceType,
                SourceId = entity.SourceId,
                SourceName = entity.SourceName,
                ExternalId = entity.ExternalId,
                ExternalStatus = entity.ExternalStatus,
                ExternalMetadata = entity.ExternalMetadata,
                CustomProperties = string.IsNullOrEmpty(entity.CustomProperties) ? new Dictionary<string, string>() : JsonConvert.DeserializeObject<Dictionary<string, string>>(entity.CustomProperties, JsonSettings.CaseSensitive),
                ExtendableSearchablePropertyKeys = string.IsNullOrEmpty(entity.ExtendableSearchablePropertyKeys) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(entity.ExtendableSearchablePropertyKeys, JsonSettings.CaseSensitive),
                ExternalCreatedDate = entity.ExternalCreatedDate,
                ExternalUpdatedDate = entity.ExternalUpdatedDate,
                LastUpdatedByExternalSource = entity.LastUpdatedByExternalSource,
                ComputedCreatedDate = entity.ComputedCreatedDate,
                ComputedUpdatedDate = entity.ComputedUpdatedDate,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Category = TicketCategoryEntity.MapToModel(entity.Category),
                CategoryId = entity.CategoryId,
                Occurrence = entity.Occurrence,
                ScheduledDate = entity.ScheduledDate,
                TemplateId = entity.TemplateId,
                Notes = entity.Notes,
                TwinId = entity.TwinId,
                Comments = CommentEntity.MapToModels(entity.Comments),
                Attachments = AttachmentEntity.MapToModels(entity.Attachments),
                Tasks = TicketTaskEntity.MapToModels(entity.Tasks),
                SubStatusId = entity.SubStatusId,
                JobTypeId = entity.JobTypeId,
                JobType = JobTypeEntity.MapToModel(entity.JobType),
                SpaceTwinId = entity.SpaceTwinId,
                ServiceNeededId = entity.ServiceNeededId,
                ServiceNeeded = ServiceNeededEntity.MapToModel(entity.ServiceNeeded),
                Diagnostics = entity.Diagnostics?.Select(x => new Insight() { Id = x.InsightId, Name = x.InsightName }).ToList()
            };
        }

        public static List<Ticket> MapToModels(IEnumerable<TicketEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public List<string> GetTrackedColumns()
        {
            var trackedColumns = new List<string>();
            // track status for all tickets created in Willow or Mapped
            if (SourceType == SourceType.Platform || SourceType == SourceType.Mapped)
            {
                trackedColumns.Add(nameof(Status));
            }
            // track other columns only if insight has a value
            if (InsightId.HasValue)
            {
                trackedColumns.AddRange(new List<string>
                {
                    nameof(AssigneeId) ,
                    nameof(AssigneeName) ,
                    nameof(AssigneeType),
                    nameof(Status),
                    nameof(Description),
                    nameof(Summary),
                    nameof(Priority),
                    nameof(DueDate)
                });

            }
            return trackedColumns.Distinct().ToList();
        }
    }
}

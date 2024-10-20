using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using WorkflowCore.Infrastructure.Json;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_CheckRecords")]
    public class CheckRecordEntity
	{
		public Guid Id { get; set; }
	
		public Guid InspectionId { get; set; }
	
		public Guid CheckId { get; set; }
	
		public Guid InspectionRecordId { get; set; }
	
		public CheckRecordStatus Status { get; set; }
	
		public Guid? SubmittedUserId { get; set; }

		public DateTime? SubmittedDate { get; set; }

		public DateTime? SubmittedSiteLocalDate { get; set; }

        public DateTime? SyncedDate { get; set; }

        public DateTime? SyncedSiteLocalDate { get; set; }

        public double? NumberValue { get; set; }
        [MaxLength(512)]
        public string TypeValue { get; set; }

        [MaxLength(216)]
		public string StringValue { get; set; }

        public DateTime? DateValue { get; set; }

		[MaxLength(1024)]
		public string Notes { get; set; }

		public Guid? InsightId { get; set; }

        public DateTime EffectiveDate { get; set; }

        public string Attachments { get; set; }

        public static CheckRecord MapToModel(CheckRecordEntity entity)
        {
            return new CheckRecord
            {
                Id = entity.Id,
                InspectionId = entity.InspectionId,
                CheckId = entity.CheckId,
                InspectionRecordId = entity.InspectionRecordId,
                Status = entity.Status,
                SubmittedUserId = entity.SubmittedUserId,
                SubmittedDate = entity.SubmittedDate,
                SubmittedSiteLocalDate = entity.SubmittedSiteLocalDate,
                SyncedDate= entity.SyncedDate,
                SyncedSiteLocalDate= entity.SyncedSiteLocalDate,
                TypeValue = entity.TypeValue,
                NumberValue = entity.NumberValue,
                StringValue = entity.StringValue,
                DateValue = entity.DateValue,
                Notes = entity.Notes,
                InsightId = entity.InsightId,
                EffectiveDate = entity.EffectiveDate,
                Attachments = string.IsNullOrEmpty(entity.Attachments)
                ? new List<AttachmentBase>()
                : JsonSerializer.Deserialize<List<AttachmentBase>>(entity.Attachments, JsonSerializerExtensions.DefaultOptions)
            };
        }

        public static List<CheckRecord> MapToModels(IEnumerable<CheckRecordEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static CheckRecordEntity MapFromModel(CheckRecord model,string typeValue)
        {
            return new CheckRecordEntity
            {
                Id = model.Id,
                InspectionId = model.InspectionId,
                CheckId = model.CheckId,
                InspectionRecordId = model.InspectionRecordId,
                Status = model.Status,
                SubmittedUserId = model.SubmittedUserId,
                SubmittedDate = model.SubmittedDate,
                SubmittedSiteLocalDate = model.SubmittedSiteLocalDate,
                SyncedDate= model.SyncedDate,
                SyncedSiteLocalDate= model.SyncedSiteLocalDate,
                TypeValue = model.TypeValue,
                NumberValue = model.NumberValue,
                StringValue = model.StringValue,
                DateValue = model.DateValue,
                Notes = model.Notes,
                InsightId = model.InsightId,
                EffectiveDate = model.EffectiveDate,
                Attachments = JsonSerializer.Serialize(model.Attachments, JsonSerializerExtensions.DefaultOptions)
            };
        }
    }
}

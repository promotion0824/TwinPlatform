using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;
using WorkflowCore.Services.Apis;

namespace WorkflowCore.Dto
{
    public class CheckRecordDto
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
        public Guid CheckId { get; set; }
        public Guid InspectionRecordId { get; set; }
        public CheckRecordStatus Status { get; set; }
        public Guid? SubmittedUserId { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? SubmittedSiteLocalDate { get; set; }
        public double? NumberValue { get; set; }
        public string TypeValue { get; set; }
        public string StringValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string Notes { get; set; }
        public Guid? InsightId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<AttachmentDto> Attachments { get; set; }

        public static CheckRecordDto MapFromModel(CheckRecord model)
        {
            if (model == null)
            {
                return null;
            }

            return new CheckRecordDto
            {
                Id = model.Id,
                InspectionId = model.InspectionId,
                CheckId = model.CheckId,
                InspectionRecordId = model.InspectionRecordId,
                Status = model.Status,
                SubmittedUserId = model.SubmittedUserId,
                SubmittedDate = model.SubmittedDate,
                SubmittedSiteLocalDate = model.SubmittedSiteLocalDate,
                NumberValue = model.NumberValue,
                TypeValue = model.TypeValue,
                StringValue = model.StringValue,
                DateValue = model.DateValue,
                Notes = model.Notes,
                InsightId = model.InsightId,
                EffectiveDate = model.EffectiveDate
            };
        }

        public static List<CheckRecordDto> MapFromModels(List<CheckRecord> models)
        {
            return models?.Select(MapFromModel).ToList();
        }

        public static CheckRecordDto MapFromModel(CheckRecord model, IImagePathHelper helper, Guid customerId, Guid siteId)
        {
            var dto = MapFromModel(model);
            if (dto == null)
            {
                return null;
            }

            dto.Attachments = AttachmentDto.MapFromCheckRecordModels(model.Attachments, helper, customerId, siteId, model.Id);
            return dto;
        }

        public static List<CheckRecordDto> MapFromModels(List<CheckRecord> models, IImagePathHelper helper, Guid customerId, Guid siteId)
        {
            return models?.Select(x => MapFromModel(x, helper, customerId, siteId)).ToList();
        }
    }
}

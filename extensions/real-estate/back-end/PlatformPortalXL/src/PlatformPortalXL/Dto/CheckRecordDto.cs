using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
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
        public string EnteredBy { get; set; }
        public double? NumberValue { get; set; }
        public string StringValue { get; set; }
        public DateTime? DateValue { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Notes { get; set; }
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
                StringValue = model.StringValue,
                DateValue = model.DateValue,
                EffectiveDate = model.EffectiveDate,
                Notes = model.Notes,
            };
        }

        public static CheckRecordDto MapFromModel(CheckRecord model, IImageUrlHelper helper)
        {
            var dto = MapFromModel(model);
            if (dto == null)
            {
                return null;
            }

            dto.Attachments = AttachmentDto.MapFromModels(model.Attachments, helper);
            return dto;
        }

        public static List<CheckRecordDto> MapFromModels(List<CheckRecord> models, IImageUrlHelper helper)
        {
            return models?.Select(x => MapFromModel(x, helper)).ToList();
        }
    }
}

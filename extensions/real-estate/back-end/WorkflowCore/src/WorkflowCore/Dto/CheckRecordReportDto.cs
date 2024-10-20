using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;
using WorkflowCore.Services.Apis;

namespace WorkflowCore.Dto
{
    public class CheckRecordReportDto
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
        public string InspectionName { get; set; }
        public Guid CheckId { get; set; }
        public string CheckName { get; set; }
        public Guid ZoneId { get; set; }
        public string ZoneName { get; set; }
        public string FloorCode { get; set; }
        public string TwinName { get; set; }
        public Guid InspectionRecordId { get; set; }
        public CheckRecordStatus Status { get; set; }
        public Guid? SubmittedUserId { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? SubmittedSiteLocalDate { get; set; }
        public double? NumberValue { get; set; }
        public string StringValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string Notes { get; set; }
        public Guid? InsightId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public CheckType CheckType { get; set; }
        public string TypeValue { get; set; }
        public List<AttachmentDto> Attachments { get; set; }

        public static CheckRecordReportDto MapFromModel(CheckRecord model,Inspection inspection,string twinName)
        {
            if (model == null || inspection==null)
            {
                return null;
            }

            var check = inspection.Checks.FirstOrDefault(c => c.Id == model.CheckId);
            return new CheckRecordReportDto()
            {
                Id = model.Id,
                InspectionId = model.InspectionId,
                InspectionName = inspection.Name,
                FloorCode = inspection.FloorCode,
                CheckId = model.CheckId,
                CheckName = check?.Name,
                CheckType = check?.Type??CheckType.Numeric,
                TypeValue = model.TypeValue??check?.TypeValue,
                ZoneId = inspection.ZoneId,
                ZoneName = inspection.Zone?.Name,
                TwinName = twinName,
                InspectionRecordId = model.InspectionRecordId,
                Status = model.Status,
                SubmittedUserId = model.SubmittedUserId,
                SubmittedDate = model.SubmittedDate,
                SubmittedSiteLocalDate = model.SubmittedSiteLocalDate,
                NumberValue = model.NumberValue,
                StringValue = model.StringValue,
                DateValue = model.DateValue,
                Notes = model.Notes,
                InsightId = model.InsightId,
                EffectiveDate = model.EffectiveDate
            };
        }

        public static CheckRecordReportDto MapFromModel(CheckRecord model, IImagePathHelper helper, Inspection inspection, string twinName,Guid customerId, Guid siteId)
        {
            var dto = MapFromModel(model, inspection, twinName);
            if (dto == null)
            {
                return null;
            }

            dto.Attachments = AttachmentDto.MapFromCheckRecordModels(model.Attachments, helper, customerId, siteId, model.Id);
            return dto;
        }

        public static List<CheckRecordReportDto> MapFromModels(List<CheckRecord> models, IImagePathHelper helper, Inspection inspection, string twinName, Guid customerId, Guid siteId)
        {
            return models?.Select(x => MapFromModel(x, helper,inspection,twinName, customerId, siteId)).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Services;
using Willow.Workflow;

namespace PlatformPortalXL.Dto
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
        public string EnteredBy { get; set; }
        public List<AttachmentDto> Attachments { get; set; }

        public static CheckRecordReportDto MapFromModel(CheckRecordReport model, string enteredBy)
        {
            if (model == null)
            {
                return null;
            }

            return new CheckRecordReportDto
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
                CheckType = model.CheckType,
                CheckName = model.CheckName,
                TypeValue = model.TypeValue,
                FloorCode = model.FloorCode,
                InsightId = model.InsightId,
                InspectionName = model.InspectionName,
                TwinName = model.TwinName,
                ZoneId = model.ZoneId,
                ZoneName = model.ZoneName,
                EnteredBy = enteredBy
            };
        }


        public static List<CheckRecordReportDto> MapFromModels(List<CheckRecordReport> models, IImageUrlHelper helper, Dictionary<Guid, string> siteUserDict)
        {
            var result= new List<CheckRecordReportDto>();  
            foreach (var model in models)
            {
                var enteredBy =  siteUserDict.TryGetValue(model.SubmittedUserId ?? Guid.Empty, out var value)
                    ? value
                    : "Unknown";
                var dto = MapFromModel(model, enteredBy);
                if (dto == null)
                {
                    return null;
                }

                dto.Attachments = AttachmentDto.MapFromModels(model.Attachments, helper);
               result.Add(dto);
            }
            return result;
        }
    }
}

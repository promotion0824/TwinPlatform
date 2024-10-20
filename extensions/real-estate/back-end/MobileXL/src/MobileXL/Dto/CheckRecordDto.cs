using MobileXL.Models;
using MobileXL.Models.Enums;
using MobileXL.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileXL.Dto
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
        public string StringValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string Notes { get; set; }
        public Guid? InsightId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
		public CustomerUser EnteredBy { get; set; }

		public static CheckRecordDto Map(CheckRecord checkRecord, IImageUrlHelper helper)
        {
            if (checkRecord == null)
            {
                return null;
            }

            return new CheckRecordDto
            {
                Id = checkRecord.Id,
                InspectionId = checkRecord.InspectionId,
                CheckId = checkRecord.CheckId,
                InspectionRecordId = checkRecord.InspectionRecordId,
                Status = checkRecord.Status,
                SubmittedUserId = checkRecord.SubmittedUserId,
                SubmittedDate = checkRecord.SubmittedDate,
                SubmittedSiteLocalDate = checkRecord.SubmittedSiteLocalDate,
                NumberValue = checkRecord.NumberValue,
                StringValue = checkRecord.StringValue,
                DateValue = checkRecord.DateValue,
                Notes = checkRecord.Notes,
                InsightId = checkRecord.InsightId,
                EffectiveDate = checkRecord.EffectiveDate,
                Attachments = AttachmentDto.MapFromModels(checkRecord.Attachments, helper)
            };
        }

        public static List<CheckRecordDto> Map(IEnumerable<CheckRecord> checkRecords, IImageUrlHelper helper)
        {
            return checkRecords?.Select(x => Map(x, helper)).ToList();
        }
    }
}

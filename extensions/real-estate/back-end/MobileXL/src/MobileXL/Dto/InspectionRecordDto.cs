using MobileXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Services;

namespace MobileXL.Dto
{
    public class InspectionRecordDto
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
		public DateTime EffectiveAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public InspectionDto Inspection { get; set; }
        public List<CheckRecordDto> CheckRecords { get; set; }

        public static InspectionRecordDto Map(InspectionRecord inspectionRecord, IImageUrlHelper helper)
        {
            if (inspectionRecord == null)
            {
                return null;
            }

            return new InspectionRecordDto
            {
                Id = inspectionRecord.Id,
                InspectionId = inspectionRecord.InspectionId,
				EffectiveAt = inspectionRecord.EffectiveAt,
				ExpiresAt= inspectionRecord.ExpiresAt,
                Inspection = inspectionRecord.Inspection == null ? null : InspectionDto.Map(inspectionRecord.Inspection, helper),
                CheckRecords = inspectionRecord.CheckRecords == null ? null : CheckRecordDto.Map(inspectionRecord.CheckRecords, helper)
            };
        }

        public static IList<InspectionRecordDto> Map(IList<InspectionRecord> inspectionRecords, IImageUrlHelper helper)
        {
            return inspectionRecords?.Select(x => Map(x, helper)).ToList();
        }
    }
}

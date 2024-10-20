using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;
using MobileXL.Models.Enums;
using MobileXL.Services;

namespace MobileXL.Dto
{
    public class InspectionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ZoneId { get; set; }
        public Guid AssetId { get; set; }
        public string TwinId { get; set; }
        public string AssetName { get; set; }
        public string FloorCode { get; set; }
        public Guid AssignedWorkgroupId { get; set; }
        public int SortOrder { get; set; }
		public int Frequency { get; set; }
		public SchedulingUnit FrequencyUnit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
		public List<CheckDto> Checks { get; set; }
        public InspectionRecordDto LastRecord { get; set; }
        public List<InspectionRecordDto> InspectionRecords { get; set; }
        public CheckRecordStatus? CheckRecordSummaryStatus { get; set; }
        public DateTime? NextCheckRecordDueTime { get; set; }

        public static InspectionDto Map(Inspection inspection, IImageUrlHelper helper)
        {
            if (inspection == null)
            {
                return null;
            }

            return new InspectionDto
            {
                Id = inspection.Id,
                Name = inspection.Name,
                FloorCode = inspection.FloorCode,
                ZoneId = inspection.ZoneId,
                AssetId = inspection.AssetId,
                TwinId = inspection.TwinId,
                AssignedWorkgroupId = inspection.AssignedWorkgroupId,
                Frequency = inspection.Frequency,
				FrequencyUnit= inspection.FrequencyUnit,
                StartDate = inspection.StartDate,
                EndDate  = inspection.EndDate,
                SortOrder = inspection.SortOrder,
                Checks = inspection.Checks == null ? null : CheckDto.Map(inspection.Checks, helper),
                LastRecord = inspection.LastRecord == null ? null : InspectionRecordDto.Map(inspection.LastRecord, helper),
                CheckRecordSummaryStatus = inspection.CheckRecordSummaryStatus,
                NextCheckRecordDueTime = inspection.NextCheckRecordDueTime
            };
        }

        public static IList<InspectionDto> Map(IList<Inspection> inspections, IImageUrlHelper helper)
        {
            return inspections?.Select(x => Map(x, helper)).ToList();
        }
    }
}

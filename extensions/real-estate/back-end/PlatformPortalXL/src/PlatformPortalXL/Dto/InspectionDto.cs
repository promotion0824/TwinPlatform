using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class InspectionDto
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string Name { get; set; }
        public Guid ZoneId { get; set; }
        public string FloorCode { get; set; }
        [Obsolete("Use TwinId instead")]
        public Guid AssetId { get; set; }
        public string TwinId { get; set; }
        public Guid AssignedWorkgroupId { get; set; }
        public int Frequency { get; set; }
        public SchedulingUnit FrequencyUnit { get; set; }
        public List<DayOfWeek> FrequencyDaysOfWeek { get; set; }
        public string NextEffectiveDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int SortOrder { get; set; }
        public List<CheckDto> Checks { get; set; }
        public int CheckRecordCount { get; set; }
        public int WorkableCheckCount { get; set; }
        public int CompletedCheckCount { get; set; }
        public DateTime? NextCheckRecordDueTime { get; set; }
        public string AssignedWorkgroupName { get; set; }
        public string ZoneName { get; set; }
        public string AssetName { get; set; }
        public CheckRecordStatus? CheckRecordSummaryStatus { get; set; }

        public static InspectionDto MapFromModel(Inspection model)
        {
            if (model == null)
            { 
                return null; 
            }

            return new InspectionDto
            {
                Id = model.Id,
                SiteId = model.SiteId,
                Name = model.Name,
                ZoneId = model.ZoneId,
                FloorCode = model.FloorCode,
                AssetId = model.AssetId,
                TwinId = model.TwinId,
                AssignedWorkgroupId = model.AssignedWorkgroupId,
                Frequency = model.Frequency,
                FrequencyUnit = model.FrequencyUnit,
                FrequencyDaysOfWeek = model.FrequencyDaysOfWeek,
                NextEffectiveDate = model.NextEffectiveDate,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                SortOrder = model.SortOrder,
                Checks = CheckDto.MapFromModels(model.Checks),
                CheckRecordCount = model.CheckRecordCount,
                WorkableCheckCount = model.WorkableCheckCount,
                CompletedCheckCount = model.CompletedCheckCount,
                NextCheckRecordDueTime = model.NextCheckRecordDueTime,
                CheckRecordSummaryStatus = model.CheckRecordSummaryStatus
            };
        }

        public static List<InspectionDto> MapFromModels(List<Inspection> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}

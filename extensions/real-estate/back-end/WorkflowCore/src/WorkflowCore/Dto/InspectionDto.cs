using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Dto
{
    public class InspectionDto
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string Name { get; set; }
        public string FloorCode { get; set; }
        public Guid ZoneId { get; set; }
        public Guid AssetId { get; set; }
        public string TwinId { get; set; }
        public Guid AssignedWorkgroupId { get; set; }
        public int Frequency { get; set; }
        public IEnumerable<DayOfWeek> FrequencyDaysOfWeek { get; set; }
        public SchedulingUnit FrequencyUnit { get; set; }
		[SwaggerSchema(Description="This is in the site timezone")]
        public string StartDate { get; set; }
		[SwaggerSchema(Description = "This is in the site timezone")]
		public string EndDate { get; set; }
        public bool IsArchived { get; set; }
        public int SortOrder { get; set; }
        public List<CheckDto> Checks { get; set; }
        public int CheckRecordCount { get; set; }
		public CheckRecordStatus? CheckRecordSummaryStatus { get; set; }
		public DateTime? NextCheckRecordDueTime { get; set; }
        public DateTime? LastCheckSubmittedDate { get; set; }
        public int WorkableCheckCount { get; set; }
        public int CompletedCheckCount { get; set; }

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
                FloorCode = model.FloorCode,
                ZoneId = model.ZoneId,
                AssetId = model.AssetId,
                TwinId = model.TwinId,
                AssignedWorkgroupId = model.AssignedWorkgroupId,
                Frequency = model.Frequency,
                FrequencyUnit = model.FrequencyUnit,
                FrequencyDaysOfWeek = model.FrequencyDaysOfWeek,
                StartDate = model.StartDate.ToString("s"),
                EndDate = model.EndDate.HasValue ? model.EndDate.Value.ToString("s") : null,
                IsArchived = model.IsArchived,
                SortOrder = model.SortOrder,
                Checks = CheckDto.MapFromModels(model.Checks),
                CheckRecordCount = model.CheckRecordCount,
                CheckRecordSummaryStatus = model.CheckRecordSummaryStatus,
                NextCheckRecordDueTime = model.NextCheckRecordDueTime,
                LastCheckSubmittedDate = model.LastCheckSubmittedDate,
                CompletedCheckCount = model.CompletedCheckCount,
                WorkableCheckCount = model.WorkableCheckCount
            };
        }

        public static List<InspectionDto> MapFromModels(List<Inspection> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}

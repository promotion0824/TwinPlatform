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
    [Table("WF_Inspections")]
    public class InspectionEntity
    {
        public Guid Id { get; set; }

		public Guid SiteId { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(10)]
		public string FloorCode { get; set; }
	 
		public Guid ZoneId { get; set; }
	 
		public Guid AssetId { get; set; }
		[MaxLength(250)]
		public string TwinId { get; set; }

		public Guid AssignedWorkgroupId { get; set; }
	 
		public int Frequency { get; set; }

        public SchedulingUnit FrequencyUnit { get; set; }
        [MaxLength(100)]
        public string FrequencyDaysOfWeekJson { get; set; }
        public DateTime StartDate { get; set; }
	 
		public DateTime? EndDate { get; set; }

		public Guid? LastRecordId { get; set; }

      //  public DateTime NextEffectiveDate { get; set; }

        public bool IsArchived { get; set; }

        public int SortOrder { get; set; }

        public List<CheckEntity> Checks { get; set; }
        [ForeignKey("ZoneId")]
        public ZoneEntity Zone { get; set; }
        [ForeignKey("LastRecordId")]
        public InspectionRecordEntity LastRecord { get; set; }

        public static Inspection MapToModel(InspectionEntity entity)
        {
            return new Inspection
            {
                Id = entity.Id,
                SiteId = entity.SiteId,
                Name = entity.Name,
                FloorCode = entity.FloorCode,
                ZoneId = entity.ZoneId,
                AssetId = entity.AssetId,
				TwinId = entity.TwinId,
                AssignedWorkgroupId = entity.AssignedWorkgroupId,
                Frequency = entity.Frequency,
                FrequencyUnit = entity.FrequencyUnit,
                FrequencyDaysOfWeek = string.IsNullOrEmpty(entity.FrequencyDaysOfWeekJson) ? null : JsonSerializer.Deserialize<IEnumerable<DayOfWeek>>(entity.FrequencyDaysOfWeekJson, JsonSerializerExtensions.DefaultOptions),
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                LastRecordId = entity.LastRecordId,
                IsArchived = entity.IsArchived,
                SortOrder = entity.SortOrder,
                Zone=ZoneEntity.MapToModel(entity.Zone),
                Checks = CheckEntity.MapToModels(entity.Checks),
                LastRecord = entity.LastRecord == null ? null : InspectionRecordEntity.MapToModel(entity.LastRecord)
            };
        }

        public static List<Inspection> MapToModels(IEnumerable<InspectionEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static InspectionEntity MapFromModel(Inspection model)
        {
            return new InspectionEntity
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
                FrequencyDaysOfWeekJson = model.FrequencyDaysOfWeek is not null ? JsonSerializer.Serialize(model.FrequencyDaysOfWeek, JsonSerializerExtensions.DefaultOptions) : null,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                LastRecordId = model.LastRecordId,
                IsArchived = model.IsArchived,
                SortOrder = model.SortOrder
            };
        }

		public static List<InspectionEntity> MapFromModels(List<Inspection> inspections)
		{
			return inspections.Select(MapFromModel).ToList();
		}
     
}
}

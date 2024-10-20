using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_Checks")]
	public class CheckEntity
	{
		public Guid Id { get; set; }

		public Guid InspectionId { get; set; }

		public int SortOrder { get; set; }

		[MaxLength(200)]
		public string Name { get; set; }

		public CheckType Type { get; set; }

		[MaxLength(512)]
		public string TypeValue { get; set; }

		public int DecimalPlaces { get; set; }

		public double? MinValue { get; set; }

		public double? MaxValue { get; set; }

        public double Multiplier { get; set; } = 1;

        public Guid? DependencyId { get; set; }

		[MaxLength(50)]
		public string DependencyValue { get; set; }

		public DateTime? PauseStartDate { get; set; }

		public DateTime? PauseEndDate { get; set; }

		public Guid? LastRecordId { get; set; }

		public Guid? LastSubmittedRecordId { get; set; }

		public bool IsArchived { get; set; }

        public bool CanGenerateInsight { get; set; }

        [ForeignKey("LastRecordId")]
        public CheckRecordEntity LastRecord { get; set; }
        [ForeignKey("LastSubmittedRecordId")]
        public CheckRecordEntity LastSubmittedRecord { get; set; }

        public static Check MapToModel(CheckEntity entity)
        {
            return new Check
            {
                Id = entity.Id,
                InspectionId = entity.InspectionId,
                SortOrder = entity.SortOrder,
                Name = entity.Name,
                Type = entity.Type,
                TypeValue = entity.TypeValue,
                DecimalPlaces = entity.DecimalPlaces,
                MinValue = entity.MinValue,
                MaxValue = entity.MaxValue,
                Multiplier = entity.Multiplier,
                DependencyId = entity.DependencyId,
                DependencyValue = entity.DependencyValue,
                PauseStartDate = entity.PauseStartDate,
                PauseEndDate = entity.PauseEndDate,
                LastRecordId = entity.LastRecordId,
                LastSubmittedRecordId = entity.LastSubmittedRecordId,
                IsArchived = entity.IsArchived,
                CanGenerateInsight = entity.CanGenerateInsight,

                LastRecord = entity.LastRecord == null ? null : CheckRecordEntity.MapToModel(entity.LastRecord),
                LastSubmittedRecord = entity.LastSubmittedRecord == null ? null : CheckRecordEntity.MapToModel(entity.LastSubmittedRecord)
            };
        }

        public static List<Check> MapToModels(IEnumerable<CheckEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static CheckEntity MapFromModel(Check model)
        {
            return new CheckEntity
            {
                Id = model.Id,
                InspectionId = model.InspectionId,
                SortOrder = model.SortOrder,
                Name = model.Name,
                Type = model.Type,
                TypeValue = model.TypeValue,
                DecimalPlaces = model.DecimalPlaces,
                MinValue = model.MinValue,
                MaxValue = model.MaxValue,
                Multiplier = model.Multiplier,
                DependencyId = model.DependencyId,
                DependencyValue = model.DependencyValue,
                PauseStartDate = model.PauseStartDate,
                PauseEndDate = model.PauseEndDate,
                LastRecordId = model.LastRecordId,
                LastSubmittedRecordId = model.LastSubmittedRecordId,
                IsArchived = model.IsArchived,
                CanGenerateInsight = model.CanGenerateInsight
            };
        }
    }
}

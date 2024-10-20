using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Dto
{
    public class CheckDto
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public CheckType Type { get; set; }
        public string TypeValue { get; set; }
        public int DecimalPlaces { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public double Multiplier { get; set; } 
        public Guid? DependencyId { get; set; }
        public string DependencyValue { get; set; }
        public DateTime? PauseStartDate { get; set; }
        public DateTime? PauseEndDate { get; set; }
        public bool IsArchived { get; set; }
        public bool CanGenerateInsight { get; set; }

        public CheckRecordDto LastSubmittedRecord { get; set; }
        public CheckStatistics Statistics { get; set; }

        public static CheckDto MapFromModel(Check model)
        {
            if (model == null)
            {
                return null;
            }

            return new CheckDto
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
                IsArchived = model.IsArchived,
                LastSubmittedRecord = CheckRecordDto.MapFromModel(model.LastSubmittedRecord),
                Statistics = model.Statistics,
                CanGenerateInsight = model.CanGenerateInsight
            };
        }

        public static List<CheckDto> MapFromModels(List<Check> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}

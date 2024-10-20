using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class CheckDto
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
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
        public bool IsPaused { get; set; }
        public bool CanGenerateInsight { get; set; }

        public CheckRecordDto LastSubmittedRecord { get; set; }
        public InspectionCheckStatistics Statistics { get; set; }

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
                Name = model.Name,
                Type = model.Type.ToString(),
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
                CanGenerateInsight = model.CanGenerateInsight,
                LastSubmittedRecord = CheckRecordDto.MapFromModel(model.LastSubmittedRecord),
                Statistics = model.Statistics
            };
        }

        public static List<CheckDto> MapFromModels(List<Check> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}

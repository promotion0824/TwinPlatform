using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;
using MobileXL.Models.Enums;
using MobileXL.Services;

namespace MobileXL.Dto
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

        public CheckRecordDto LastSubmittedRecord { get; set; }

        public static CheckDto Map(Check check, IImageUrlHelper helper)
        {
            if (check == null)
            {
                return null;
            }

            return new CheckDto
            {
                Id = check.Id,
                InspectionId = check.InspectionId,
                SortOrder = check.SortOrder,
                Name = check.Name,
                Type = check.Type,
                TypeValue = check.TypeValue,
                DecimalPlaces = check.DecimalPlaces,
                MinValue = check.MinValue,
                MaxValue = check.MaxValue,
                Multiplier = check.Multiplier,
                DependencyId = check.DependencyId,
                DependencyValue = check.DependencyValue,
                PauseStartDate = check.PauseStartDate,
                PauseEndDate = check.PauseEndDate,
                LastSubmittedRecord = check.LastSubmittedRecord == null ? null : CheckRecordDto.Map(check.LastSubmittedRecord, helper),
            };
        }

        public static List<CheckDto> Map(IList<Check> checks, IImageUrlHelper helper)
        {
            return checks?.Select(x => Map(x, helper)).ToList();
        }
    }
}

using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Dto
{
    public class InspectionRecordDto
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
        public string EffectiveDate { get; set; }

        public static InspectionRecordDto MapFromModel(InspectionRecord model)
        {
            if (model == null)
            {
                return null;
            }

            return new InspectionRecordDto
            {
                Id = model.Id,
                InspectionId = model.InspectionId,
                EffectiveDate = model.EffectiveDate
            };
        }

        public static List<InspectionRecordDto> MapFromModels(List<InspectionRecord> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}

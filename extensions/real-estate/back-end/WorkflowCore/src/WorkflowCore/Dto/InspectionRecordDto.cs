using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Dto
{
    public class InspectionRecordDto
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
		public DateTime EffectiveAt { get; set; }
		public DateTime ExpiresAt { get; set; }

		public InspectionDto Inspection { get; set; }
        public List<CheckRecordDto> CheckRecords { get; set; }

        public static InspectionRecordDto MapFromModel(InspectionRecord model)
        {
            if (model == null)
            {
                return null;
            }

            return new InspectionRecordDto
            {
                Id = model.Id,
                InspectionId = model.InspectionId
            };
        }

        public static List<InspectionRecordDto> MapFromModels(List<InspectionRecord> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}

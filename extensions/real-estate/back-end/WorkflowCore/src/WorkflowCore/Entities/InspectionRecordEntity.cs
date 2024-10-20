using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_InspectionRecords")]
    public class InspectionRecordEntity
    {
        public Guid Id { get; set; }
    
        public Guid SiteId { get; set; }
    
        public Guid InspectionId { get; set; }
    
        public DateTime EffectiveDate { get; set; }

        public int Occurrence { get; set; } = 0; // Hour index of schedule hit

        public static InspectionRecord MapToModel(InspectionRecordEntity entity)
        {
            return new InspectionRecord
            {
                Id = entity.Id,
                SiteId = entity.SiteId,
                InspectionId = entity.InspectionId,
                EffectiveDate = entity.EffectiveDate,
                Occurrence = entity.Occurrence
            };
        }

        public static List<InspectionRecord> MapToModels(IEnumerable<InspectionRecordEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static InspectionRecordEntity MapFromModel(InspectionRecord model)
        {
            return new InspectionRecordEntity
            {
                Id = model.Id,
                SiteId = model.SiteId,
                InspectionId = model.InspectionId,
                EffectiveDate = model.EffectiveDate,
                Occurrence = model.Occurrence
            };
        }
    }
}

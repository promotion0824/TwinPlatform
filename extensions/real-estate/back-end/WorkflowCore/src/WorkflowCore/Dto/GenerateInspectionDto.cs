using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;
using WorkflowCore.Entities;

namespace WorkflowCore.Dto
{
    public class GenerateInspectionDto
    {
        public Guid      Id      { get; set; }
        public string    Name    { get; set; }
        public DateTime? SiteNow { get; set; }

        public static GenerateInspectionDto MapFromEntity(InspectionEntity model, DateTime siteNow)
        {
            return model == null ? null : new GenerateInspectionDto
            {
                Id      = model.Id,
                Name    = model.Name,
                SiteNow = siteNow
            };
        }
    }
}

using System;
using System.Collections.Generic;
using WorkflowCore.Models;

namespace WorkflowCore.Dto
{
    public class InspectionUsageDto
    {
        public List<string> XAxis { get; set; }
        public List<Guid> UserIds { get; set; }
        public List<List<int>> Data { get; set; }

        public static InspectionUsageDto MapFromModel(InspectionUsage model)
        {
            return new InspectionUsageDto
            {
                XAxis = model.XAxis,
                UserIds = model.UserIds,
                Data = model.Data
            };
        }
    }
}

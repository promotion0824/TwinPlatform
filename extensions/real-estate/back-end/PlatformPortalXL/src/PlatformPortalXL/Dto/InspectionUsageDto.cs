using PlatformPortalXL.Models;
using System.Collections.Generic;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class InspectionUsageDto
    {
        public List<string> XAxis { get; set; }
        public List<string> UserName { get; set; }
        public List<List<int>> Data { get; set; }

        public static InspectionUsageDto MapFromModel(InspectionUsage model)
        {
            if (model == null)
            {
                return null;
            }

            return new InspectionUsageDto
            {
                XAxis = model.XAxis,
                Data = model.Data
            };
        }
    }
}

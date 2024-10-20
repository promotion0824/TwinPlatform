using System;
using System.Collections.Generic;

namespace WorkflowCore.Models
{
    public class InspectionUsage
    {
        public List<string> XAxis { get; set; }
        public List<Guid> UserIds { get; set; }
        public List<List<int>> Data { get; set; }
    }
}

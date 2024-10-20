using System;
using System.Collections.Generic;

namespace WorkflowCore.Controllers.Request
{
    public class UpdateInspectionSortOrderRequest
    {
        public List<Guid> InspectionIds { get; set; }
    }
}

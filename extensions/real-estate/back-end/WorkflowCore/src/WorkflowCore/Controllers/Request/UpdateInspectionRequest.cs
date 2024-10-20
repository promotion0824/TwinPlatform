using System;
using System.Collections.Generic;
using WorkflowCore.Models;

namespace WorkflowCore.Controllers.Request
{
    public class UpdateInspectionRequest : InspectionRequest
    {
        public override List<CheckRequest> Checks { get; set; }
    }
}

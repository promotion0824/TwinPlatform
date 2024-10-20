using System;
using System.Collections.Generic;
using WorkflowCore.Models;

namespace WorkflowCore.Controllers.Request
{
    public class CreateInspectionRequest : InspectionRequest
    {
        public Guid ZoneId { get; set; }
        public string FloorCode { get; set; }
		[Obsolete("This field is no loner in use")]
		public Guid AssetId { get; set; }
		public string TwinId { get; set; }
public override List<CheckRequest> Checks { get; set; }
    }
}

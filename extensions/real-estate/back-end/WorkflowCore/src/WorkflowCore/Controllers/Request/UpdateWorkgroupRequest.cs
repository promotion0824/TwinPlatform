using System;
using System.Collections.Generic;

namespace WorkflowCore.Controllers.Request
{
    public class UpdateWorkgroupRequest
    {
        public string Name { get; set; }
        public List<Guid> MemberIds { get; set; }
    }
}

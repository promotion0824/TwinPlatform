using System;
using System.Collections.Generic;

namespace WorkflowCore.Controllers.Request
{
    public class CreateWorkgroupRequest
    {
        public string Name { get; set; }
        public List<Guid> MemberIds { get; set; }
    }
}

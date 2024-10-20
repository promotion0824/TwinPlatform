using System;
using System.Collections.Generic;

namespace Willow.Workflow
{
    public class UpdateWorkgroupRequest
    {
        public string Name { get; set; }
        public List<Guid> MemberIds { get; set; }
    }
}

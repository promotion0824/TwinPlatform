using System;
using System.Collections.Generic;

namespace WorkflowCore.Models
{
    public class Workgroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid SiteId { get; set; }
        public List<Guid> MemberIds { get; set; }
    }
}

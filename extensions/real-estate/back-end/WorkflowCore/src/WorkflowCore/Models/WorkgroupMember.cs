using System;

namespace WorkflowCore.Models
{
    public class WorkgroupMember
    {
        public Guid WorkgroupId { get; set; }
        public Guid MemberId { get; set; }
    }
}

using System;

namespace WorkflowCore.Models
{
    public class Developer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid OwnerUserId { get; set; }
    }
}

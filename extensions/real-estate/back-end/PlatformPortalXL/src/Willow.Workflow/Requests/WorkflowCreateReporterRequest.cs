using System;

namespace Willow.Workflow
{
    public class WorkflowCreateReporterRequest
    {
        public Guid CustomerId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
    }
}

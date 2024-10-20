using System;

namespace WorkflowCore.Controllers.Request
{
    public class CreateReporterRequest
    {
        public Guid CustomerId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
    }
}

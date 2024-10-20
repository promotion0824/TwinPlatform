using System;
using System.Collections.Generic;

namespace MobileXL.Features.Workflow
{
    public class UpdateTicketTasksRequest
    {
        public List<UpdateTicketTask> Tasks { get; set; }
        public string Notes { get; set; }
    }

    public class UpdateTicketTask
    {
        public Guid Id { get; set;}
        public bool IsCompleted { get; set; }
        public double? NumberValue { get; set; }
    }
}

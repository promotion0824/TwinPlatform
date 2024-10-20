using System;
using System.Collections.Generic;

namespace MobileXL.Services.Apis.WorkflowApi.Requests
{
    public class UpdateTicketTemplateRequest
    {
        public List<TicketTemplateTask> Tasks;
    }

    public class TicketTemplateTask
    {
        public Guid TaskId { get; set; }
        public bool IsCompleted { get; set; }
    }
}

using System.Collections.Generic;

namespace WorkflowCore.Controllers.Request
{
    public class CreateTicketStatusRequest
    {
        public class CreateTicketStatusRequestItem
        { 
            public int StatusCode { get; set; }
            public string Status { get; set; }
            public string Tab { get; set; }
            public string Color { get; set; }
        }
        public List<CreateTicketStatusRequestItem> TicketStatuses { get; set; }
    }
}

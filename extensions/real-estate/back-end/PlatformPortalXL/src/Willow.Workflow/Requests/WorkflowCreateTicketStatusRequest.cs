using System.Collections.Generic;

namespace Willow.Workflow
{
    public class WorkflowCreateTicketStatusRequest
    {
        public class WorkflowCreateTicketStatusRequestItem
        {
            public int StatusCode { get; set; }
            public string Status { get; set; }
            /** Status tab: Open/Resolved/Closed **/
            public string Tab { get; set; }
            /** Color of the status: Green/Yellow/Red **/
            public string Color { get; set; }
        }
        public List<WorkflowCreateTicketStatusRequestItem> TicketStatuses { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Willow.Scheduler
{
    public class Schedule
    {
        public Guid   Id              { get; set; }
        public bool   Active          { get; set; } = true;
        public Guid   OwnerId         { get; set; }
        public string Recurrence      { get; set; }
        public string RecipientClient { get; set; } // e.g. "WorkflowCore" 
        public string Recipient       { get; set; } // Endpoint to call 
    }
}

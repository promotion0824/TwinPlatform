using System;
using System.Collections.Generic;

using Willow.Calendar;

namespace WorkflowCore.Models
{
    public class TicketTemplate : TicketBase
    {
        public Event                    Recurrence       { get; set; }
        public Duration                 OverdueThreshold { get; set; }
        public List<TicketAsset>        Assets           { get; set; }
        public List<TicketTwin>         Twins            { get; set; }
        public List<TicketTaskTemplate> Tasks            { get; set; }
        public DataValue                DataValue        { get; set; }
        public string                   CategoryName     { get; set; }
        public Guid?                    CategoryId       { get; set; }
   }
}

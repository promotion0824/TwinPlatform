using System;

namespace WorkflowCore.Models
{
    public class TicketTwin
    {
        public string TwinId { get; set; }
        public string TwinName { get; set; }
    }    
    
    public class ScheduledTicketTwin
    {
        public Guid     CorrelationId   { get; set; }
        public Guid     TemplateId      { get; set; }
        public string   TwinId         { get; set; }
        public string   TwinName       { get; set; }
        public string   SequenceNumber  { get; set; }
        public int      Occurrence      { get; set; }
        public DateTime ScheduleHitDate { get; set; }
        public DateTime UtcNow          { get; set; }
    }
}

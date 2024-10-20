using System;

namespace Willow.Workflow
{
    public class TicketTask
    {
        public Guid Id { get; set; }
        public string TaskName { get; set; }
        public bool IsCompleted { get; set; }
        public double? NumberValue { get; set; }
        public TicketTaskType Type { get; set; }
        public int? DecimalPlaces { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public string Unit { get; set; }
    }
}

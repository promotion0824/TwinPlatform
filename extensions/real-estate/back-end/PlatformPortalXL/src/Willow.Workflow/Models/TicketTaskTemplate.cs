using System;
using System.ComponentModel.DataAnnotations;

namespace Willow.Workflow
{
    public class TicketTaskTemplate
    {
        [StringLength(300, ErrorMessage = "Task name length cannot exceed 300")]
        public string Description { get; set; }
        public TicketTaskType Type { get; set; }
        public int? DecimalPlaces { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public string Unit { get; set; }
    }
}

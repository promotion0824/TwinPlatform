using System.Collections.Generic;
using System;
using WorkflowCore.Models;

namespace WorkflowCore.Controllers.Request
{
    public abstract class InspectionRequest
    {
        public string Name { get; set; }
        public Guid AssignedWorkgroupId { get; set; }
        public int Frequency { get; set; }
        public SchedulingUnit FrequencyUnit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<DayOfWeek> FrequencyDaysOfWeek { get; set; }
        public abstract List<CheckRequest> Checks { get; set; }
    }
}

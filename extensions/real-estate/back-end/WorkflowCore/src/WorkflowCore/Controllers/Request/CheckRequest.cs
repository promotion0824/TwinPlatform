using System;
using WorkflowCore.Models;

namespace WorkflowCore.Controllers.Request
{
    public class CheckRequest
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public CheckType? Type { get; set; }
        public string TypeValue { get; set; }
        public int? DecimalPlaces { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public double Multiplier { get; set; } = 1;
        public string DependencyName { get; set; }
        public string DependencyValue { get; set; }
        public DateTime? PauseStartDate { get; set; }
        public DateTime? PauseEndDate { get; set; }
        public bool CanGenerateInsight { get; set; }
    }
}

namespace WorkflowCore.Models
{
    public class TicketTaskTemplate
    {
        public string Description { get; set; }
        public TaskType Type { get; set; }
        public int? DecimalPlaces { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public string Unit { get; set; }
    }

    public enum TaskType
    {
        Numeric,
        Checkbox
    }
}

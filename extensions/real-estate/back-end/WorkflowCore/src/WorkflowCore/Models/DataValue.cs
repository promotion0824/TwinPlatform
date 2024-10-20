namespace WorkflowCore.Models
{
    public class DataValue
    {
        public TicketTemplateDataType? Type { get; set; }
        public string EnumerationNameList { get; set; }
        public int? DecimalPlaces { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public string Value { get; set; }
    }
}

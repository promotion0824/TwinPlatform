using System;
using System.ComponentModel.DataAnnotations;
using Willow.DataValidation;

namespace Willow.Workflow
{
    public class DataValue
    {
        public TicketTemplateDataType? Type { get; set; }

        [StringLength(512, ErrorMessage = "ValueType exceeds the max length of 512")]
        [UniqueStringListIf("Type", TicketTemplateDataType.List, "|", ErrorMessage = "TypeValue contains duplicates")]
        public string EnumerationNameList { get; set; }

        [RequiredIf("Type", TicketTemplateDataType.Numeric, ErrorMessage = "DecimalPlaces is required")]
        [Range(0, 4, ErrorMessage = "DecimalPlaces must be between 1 and 4")]
        public int? DecimalPlaces { get; set; }

        public double? MinValue { get; set; }

        public double? MaxValue { get; set; }

        public string Value { get; set; }
    }
}

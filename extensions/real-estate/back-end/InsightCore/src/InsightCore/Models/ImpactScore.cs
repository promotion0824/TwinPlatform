
using System.Collections.Generic;

namespace InsightCore.Models
{
    public class ImpactScore
    {
        public static List<string> Priority = ["priority", "priority_impact"];

        public string Name { get; set; }
        /// <summary>
        /// The field id of the impact score
        /// </summary>
        public string FieldId { get; set; }
		public double Value { get; set; }
        public string Unit { get; set; }
        /// <summary>
        /// It is an Id that matches a timeseries in ADX's ExternalId
        /// </summary>
        public string ExternalId { get; set; }
        public string RuleId { get; set; }
    }
}

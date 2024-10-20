using System;
using Newtonsoft.Json;
using Willow.IoTService.Monitoring.Enums;

namespace Willow.IoTService.Monitoring.Dtos.PagerDuty
{
    public class Payload
    {
        [JsonProperty("summary")]
        public string? Summary { get; set; }

        [JsonProperty("source")]
        public string? Source { get; set; }

        [JsonIgnore]
        public PagerDutySeverity Severity { get; set; }

        [JsonProperty("severity")]
        public string? SeverityValue => Severity.ToString().ToLower();

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("custom_details")]
        public object? CustomDetails { get; set; }
    }
}
using System.Collections.Generic;
using Microsoft.Azure.Management.Monitor.Models;
using Newtonsoft.Json;

namespace Willow.IoTService.Monitoring.Dtos
{
    public class MetricResponseDto
    {
        [JsonProperty("cost")]
        public decimal Cost { get; set; }

        [JsonProperty("interval")]
        public string? Interval { get; set; }

        [JsonProperty("value")]
        public IEnumerable<Metric>? Value { get; set; }

        [JsonProperty("namespace")]
        public string? ResourceNamespace { get; set; }

        [JsonProperty("resourceregion")]
        public string? ResourceRegion { get; set; }
    }
}
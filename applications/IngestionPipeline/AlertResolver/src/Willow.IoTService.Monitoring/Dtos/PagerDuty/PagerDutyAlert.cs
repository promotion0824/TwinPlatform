using Newtonsoft.Json;

namespace Willow.IoTService.Monitoring.Dtos.PagerDuty
{
    public class PagerDutyAlert
    {
        [JsonProperty("routing_key")]
        public string? RoutingKey { get; set; }

        [JsonIgnore]
        public EventAction Action { get; set; }

        [JsonProperty("event_action")]
        public string ActionValue => Action.ToString().ToLower();

        [JsonProperty("dedup_key")]
        public string? DedupKey { get; set; }

        [JsonProperty("payload")]
        public Payload? EventPayload { get; set; }
    }
}
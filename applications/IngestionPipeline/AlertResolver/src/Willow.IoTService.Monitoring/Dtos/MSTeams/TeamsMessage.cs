using System.Collections.Generic;
using Newtonsoft.Json;

namespace Willow.IoTService.Monitoring.Dtos.MSTeams
{
    //https://docs.microsoft.com/en-us/outlook/actionable-messages/message-card-reference

    public class TeamsMessage
    {
        [JsonProperty("@type")]
        public string Type { get; } = "MessageCard";

        [JsonProperty("@context")]
        public string Context { get; } = "http://schema.org/extensions";

        public string? Title { get; set; }

        public string? Text { get; set; }

        public string? ThemeColor { get; set; }

        public List<MessageSection> Sections { get; set; } = new List<MessageSection>();
    }
}
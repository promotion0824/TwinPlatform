using System;
using System.Text.Json.Serialization;

namespace DigitalTwinCore.Dto
{
    public class LiveDataIngestPointDto
    {
        public Guid UniqueId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid AssetId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid TrendId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string ExternalId { get; set; }
    }
}

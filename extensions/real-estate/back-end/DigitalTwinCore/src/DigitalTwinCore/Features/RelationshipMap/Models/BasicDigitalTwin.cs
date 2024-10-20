using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Azure;

namespace DigitalTwinCore.Features.RelationshipMap.Models
{
    public sealed class BasicDigitalTwin : IEquatable<BasicDigitalTwin>
    {
        [JsonPropertyName("$dtId")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("trendID")]
        public string TrendId { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }

        [JsonPropertyName("phenomenon")]
        public string Phenomenon { get; set; }
        
        [JsonPropertyName("valueExpression")]
        public string ValueExpression { get; set; }

        [JsonPropertyName("position")]
        public string Position { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("tags")]
        public Dictionary<string, bool> Tags { get; set; }

        [JsonIgnore]
        public string TagString => Tags is null ? "" : string.Join(",", Tags.Keys);

        [JsonPropertyName("$etag")]
        [JsonIgnore]
        public ETag? ETag { get; set; }

        [JsonPropertyName("$metadata")]
        public DigitalTwinMetadata Metadata { get; set; }

        public bool Equals(BasicDigitalTwin other) => Id.Equals(other?.Id);
    }
}

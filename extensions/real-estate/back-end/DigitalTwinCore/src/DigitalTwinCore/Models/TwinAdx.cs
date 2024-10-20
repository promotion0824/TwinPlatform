using DigitalTwinCore.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalTwinCore.Models
{
    public class TwinAdx
    {
        [JsonExtensionData]
        public IDictionary<string, object> CustomProperties => JsonSerializer.Deserialize<IDictionary<string, object>>(Raw);

        public string Raw { get; set; }

        public string Id { get; set; }

        public TwinMetadata Metadata { get; set; }
    }
}

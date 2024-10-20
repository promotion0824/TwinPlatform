using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DigitalTwinCore.Dto
{
    public class LiveDataUpdatePointDto
    {
        // Note only one of these should  be specified at any time.
        // We could have a single string field and an IdType enum. (What we really want is a discriminated union -- F# only still.) 
        public Guid UniqueId { get; set; }
        public Guid TrendId { get; set; }
        public string ExternalId { get; set; }

        public DateTime Timestamp { get; set; }

        public JsonElement Value { get; set; }
    }

    public class LiveDataUpdateRequest
    {
        public List<LiveDataUpdatePointDto> UpdatePoints { get; set; }
    }
}

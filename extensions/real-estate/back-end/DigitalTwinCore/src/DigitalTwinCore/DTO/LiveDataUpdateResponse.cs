using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalTwinCore.Dto
{
    public class LiveDataUpdateResponse
    {
        public class PointUpdateResponse
        {
            public int Status { get; set; }
            public string PointId { get; set; }
            public Guid PointUniqueId { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public string PointExternalId { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public Guid PointTrendId { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public Guid AssetUniqueId { get; set; }
        }
        public List<PointUpdateResponse> Responses { get; set; }
        public List<TwinWithRelationshipsDto> UpdatedTwins { get; set; }
    }
}

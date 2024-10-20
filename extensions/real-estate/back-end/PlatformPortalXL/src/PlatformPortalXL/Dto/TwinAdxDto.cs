using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Helpers;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlatformPortalXL.Dto
{
    public class TwinAdxDto
    {
        [JsonExtensionData]
        public IDictionary<string, object> CustomProperties => JsonSerializerHelper.Deserialize<IDictionary<string, object>>(Raw);
        
        [JsonIgnore]
        public string Raw { get; set; }

        public string Id { get; set; }

        public TwinMetadataDto Metadata { get; set; }
    }
}

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlatformPortalXL.Features.Pilot
{
    public class TwinMetadataDto
    {
        public string ModelId { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> WriteableProperties { get; set; }
    }
}

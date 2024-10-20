using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlatformPortalXL.Features.Pilot
{
    public class RelationshipDto
    {
        public string Id { get; set; }
        public string TargetId { get; set; }
        public string SourceId { get; set; }
        public string Name { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> CustomProperties { get; set; }
    }
}

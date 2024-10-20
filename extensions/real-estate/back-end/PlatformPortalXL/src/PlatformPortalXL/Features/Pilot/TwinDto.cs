using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace PlatformPortalXL.Features.Pilot
{
    [DebuggerDisplay("{Id} | {Name}")]
    public class TwinDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Guid? SiteId { get; set; }
        public TwinMetadataDto Metadata { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> CustomProperties { get; set; }
        public string UserId { get; set; }
    }
}

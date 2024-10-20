using System.Collections.Generic;
using System;

namespace PlatformPortalXL.Features.Twins
{
    public class CognitiveSearchRequest
    {
        public IEnumerable<Guid> SiteIds { get; set; }
        public IEnumerable<string> TwinIds { get; set; }
        public bool SensorSearchEnabled { get; set; }
    }
}

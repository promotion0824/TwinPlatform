using System.Collections.Generic;
using System;

namespace DigitalTwinCore.Features.TwinsSearch.Dtos
{
    public class CognitiveSearchRequest
    {
        public IEnumerable<Guid> SiteIds { get; set; }
        public IEnumerable<string> TwinIds { get; set; }
        public bool SensorSearchEnabled { get; set; }
    }
}

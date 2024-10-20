using System;
using System.Collections.Generic;

namespace DigitalTwinCore.Dto
{
    public class LiveDataIngestPointsRequest
    {
        public List<Guid> Ids { get; set; }
        public List<Guid>TrendIds { get; set; }
        public List<string> ExternalIds { get; set; }
        public bool IncludePointsWithNoAssets { get; set; } = false;
    }
}

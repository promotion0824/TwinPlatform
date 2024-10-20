
using System;
using System.Collections.Generic;

namespace DigitalTwinCore.Dto
{
    [Serializable]
    public class AdtSiteStatsDto
    {
        public int TotalAdtInstanceTwinCount { get; set; }
        public int TotalAdtInstanceRelCount { get; set; }
        public int NoOutgoingRelTwinCount { get; set; }
        public int NoIncomingRelTwinCount { get; set; }
        public int NoIncomingOrOutgoingRelTwinCount { get; set; }
        public int SharedUniqueIdTwinCount { get; set; }

        public Dictionary<string, int> TwinCountsBySite { get; set; }
        public Dictionary<string, int> TwinCountsByFloor { get; set; }

        public List<string> NoUniqueIdTwins { get; set; }
        public List<string> NoIncomingOrOutgoingRelTwins { get; set; }
        public List<string> SharedUniqueIdTwins { get; set; }
    }
}

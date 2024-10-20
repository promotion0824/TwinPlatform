using System.Collections.Generic;

namespace PlatformPortalXL.Dto
{
    public class TimeZoneDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Offset { get; set; }
        // ex. "Australia/Brisbane"
        public Dictionary<string,string> RegionTimeZone { get; set; }
    }
}

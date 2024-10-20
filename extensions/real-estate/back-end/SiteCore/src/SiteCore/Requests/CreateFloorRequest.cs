using System;

namespace SiteCore.Requests
{
    public class CreateFloorRequest
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ModelReference { get; set; }
        public bool IsSiteWide { get; set; } = false;        
    }
}

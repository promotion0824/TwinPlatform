using System;

namespace SiteCore.Requests
{
    public class UpdateFloorRequest
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string ModelReference { get; set; }
        public bool? IsSiteWide { get; set; }
    }
}

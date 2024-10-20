using System.Collections.Generic;
using Willow.Platform.Models;

namespace PlatformPortalXL.Requests.SiteCore
{
    public class CreateUpdateWidgetRequest
    {
        public dynamic Metadata { get; set; }
        public WidgetType Type { get; set; }

        public List<WidgetPosition> Positions { get; set; }
    }
}

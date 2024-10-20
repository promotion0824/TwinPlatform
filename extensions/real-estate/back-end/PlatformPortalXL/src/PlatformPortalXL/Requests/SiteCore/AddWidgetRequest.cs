using System;

namespace PlatformPortalXL.Requests.SiteCore
{
    public class AddWidgetRequest
    {
        public Guid? WidgetId { get; set; }
        public int Position { get; set; }
    }
}

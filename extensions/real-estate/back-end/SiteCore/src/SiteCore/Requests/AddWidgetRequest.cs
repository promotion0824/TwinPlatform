using System;

namespace SiteCore.Requests
{
    public class AddWidgetRequest
    {
        public Guid WidgetId { get; set; }
        public int Position { get; set; }
    }
}

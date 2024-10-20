using System;

namespace Willow.Platform.Models
{
    public class WidgetPosition
    {
        public string ScopeId { get; set; }
        public Guid? SiteId { get; set; }
        public string SiteName { get; set; }
        public Guid? PortfolioId { get; set; }
        public int Position { get; set; }
    }
}

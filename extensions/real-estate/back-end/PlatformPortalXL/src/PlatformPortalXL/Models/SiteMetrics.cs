using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class SiteMetrics
    {
        public Guid SiteId { get; set; }
        public List<Metric> Metrics { get; set; }
    }
}

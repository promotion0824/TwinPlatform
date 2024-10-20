using System;
using System.Collections.Generic;

namespace SiteCore.Domain
{
    public class SiteMetrics
    {
        public Guid SiteId { get; set; }
        public List<Metric> Metrics { get; set; }
    }
}

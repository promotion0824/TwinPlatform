using System;
using System.Collections.Generic;

namespace SiteCore.Requests
{
    public class ImportSiteMetricsRequest
    {
        public DateTime TimeStamp { get; set; }
        public Dictionary<string, decimal> Metrics { get; set; }
    }
}

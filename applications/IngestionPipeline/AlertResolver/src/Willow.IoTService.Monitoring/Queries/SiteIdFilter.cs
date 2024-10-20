using System;
using Willow.IoTService.Monitoring.Models;

namespace Willow.IoTService.Monitoring.Queries
{
    public class SiteIdFilter : IMetricQueryFilter
    {
        public Guid SiteId { get; set; }

        public static IMetricQueryFilter For(Guid siteId)
        {
            return new SiteIdFilter { SiteId = siteId };
        }

        public static IMetricQueryFilter For(string siteId)
        {
            return For(Guid.Parse(siteId));
        }
    }
}
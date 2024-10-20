using System;
using Willow.IoTService.Monitoring.Models;

namespace Willow.IoTService.Monitoring.Queries
{
    public class CustomerIdFilter : IMetricQueryFilter
    {
        public Guid CustomerId { get; set; }

        public static IMetricQueryFilter For(Guid customerId)
        {
            return new CustomerIdFilter { CustomerId = customerId };
        }
    }
}
using System;
using Willow.IoTService.Monitoring.Models;

namespace Willow.IoTService.Monitoring.Queries
{
    public class ConnectorIdFilter : IMetricQueryFilter
    {
        public Guid ConnectorId { get; set; }

        public static IMetricQueryFilter For(string connectorId)
        {
            return For(Guid.Parse(connectorId));
        }

        public static IMetricQueryFilter For(Guid connectorId)
        {
            return new ConnectorIdFilter { ConnectorId = connectorId };
        }
    }
}
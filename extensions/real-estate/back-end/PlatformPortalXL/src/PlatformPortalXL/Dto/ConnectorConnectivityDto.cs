using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Dto
{
    public class ConnectorConnectivityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ServiceStatus Status { get; set; }
        public ServiceStatus GatewayStatus { get; set; }
        public int? ErrorCount { get; set; }
        public List<ConnectorConnectivityDataPointDto> History { get; set; }
    }

    public class ConnectorConnectivityDataPointDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int PointCount { get; set; }
        public int ErrorCount { get; set; }

        internal static ConnectorConnectivityDataPointDto MapFrom(PortfolioDashboardConnectorLog model) =>
            model == null ? null : new ConnectorConnectivityDataPointDto
            {
                Start = model.Start,
                End = model.End,
                ErrorCount = model.ErrorCount,
                PointCount = model.PointCount
            };

        internal static List<ConnectorConnectivityDataPointDto> MapFrom(List<PortfolioDashboardConnectorLog> models) => 
            models?.Select(MapFrom).ToList() ?? new List<ConnectorConnectivityDataPointDto>();
    }
}

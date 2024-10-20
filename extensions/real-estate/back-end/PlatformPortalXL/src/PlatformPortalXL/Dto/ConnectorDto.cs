using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class ConnectorDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        
        public Guid SiteId { get; set; }

        public string Configuration { get; set; }
        
        public Guid ConnectorTypeId { get; set; }

        public int ErrorThreshold { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsLoggingEnabled { get; set; }
        
        public string ConnectionType { get; set; }
        
        public int PointsCount { get; set; }

        public bool IsArchived { get; set; }

        public ServiceStatus? Status { get; set; }

        public static ConnectorDto MapFrom(Connector connector)
        {
            if (connector == null)
            {
                return null;
            }

            return new ConnectorDto
            {
                Id = connector.Id,
                Name = connector.Name,
                IsEnabled = connector.IsEnabled,
                Configuration = connector.Configuration,
                ErrorThreshold = connector.ErrorThreshold,
                SiteId = connector.SiteId,
                ConnectorTypeId = connector.ConnectorTypeId,
                IsLoggingEnabled = connector.IsLoggingEnabled,
                ConnectionType = connector.ConnectionType,
                PointsCount = connector.PointsCount,
                IsArchived = connector.IsArchived
            };
        }

        public static List<ConnectorDto> MapFrom(IEnumerable<Connector> connectors)
        {
            return connectors.Select(MapFrom).ToList();
        }
    }
}

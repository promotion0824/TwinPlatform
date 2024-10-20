using System;

namespace PlatformPortalXL.Models
{
    public class Connector
    {
        /// <summary>
        /// Id of the connector
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Connector's name
        /// </summary>
        public string Name { get; set; }

        public Guid ClientId { get; set; }

        public Guid SiteId { get; set; }

        public string Configuration { get; set; }

        /// <summary>
        /// Id of connector's type
        /// </summary>
        public Guid ConnectorTypeId { get; set; }

        /// <summary>
        /// Error threshold
        /// </summary>
        public int ErrorThreshold { get; set; }

        /// <summary>
        /// Flag marking enabled connectors
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Flag marking if logging is enabled for the connector
        /// </summary>
        public bool IsLoggingEnabled { get; set; }

        public string RegistrationId { get; set; }

        public string RegistrationKey { get; set; }
        
        public string ConnectionType { get; set; }
        
        public int PointsCount { get; set; }

        public bool IsArchived { get; set; }
    }
}

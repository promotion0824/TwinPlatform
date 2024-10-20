namespace ConnectorCore.Models
{
    using System;
    using System.Collections.Generic;
    using ConnectorCore.Entities;

    /// <summary>
    /// Represents the connectivity statistics for a site.
    /// </summary>
    public class ConnectivityStatistics
    {
        /// <summary>
        /// Gets or sets the site ID.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets or sets the list of gateways.
        /// </summary>
        public List<GatewayEntity> Gateways { get; set; }

        /// <summary>
        /// Gets or sets the list of connectors.
        /// </summary>
        public List<ConnectorEntity> Connectors { get; set; }
    }
}

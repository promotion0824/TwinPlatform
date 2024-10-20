namespace ConnectorCore.Entities
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a gateway entity.
    /// </summary>
    public class GatewayEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the gateway.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the gateway.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the customer ID associated with the gateway.
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the site ID associated with the gateway.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets or sets the host name or address of the gateway.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the gateway is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the last heartbeat time of the gateway.
        /// </summary>
        public DateTime? LastHeartbeatTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the gateway is online.
        /// </summary>
        public bool? IsOnline { get; set; }

        /// <summary>
        /// Gets or sets the collection of connectors associated with the gateway.
        /// </summary>
        public ICollection<ConnectorEntity> Connectors { get; set; }
    }
}

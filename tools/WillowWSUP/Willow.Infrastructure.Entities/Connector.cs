#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents a connector instance.
    /// </summary>
    public class Connector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Connector"/> class.
        /// </summary>
        public Connector()
        {
            BuildingConnectors = new HashSet<BuildingConnector>();
        }

        /// <summary>
        /// Gets the connector id
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the Customer Instance id
        /// </summary>
        public Guid CustomerInstanceId { get; init; }

        /// <summary>
        /// Gets the connector name
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the connector description
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets the status of the connector.
        /// </summary>
        public int ConnectorStatusId { get; init; }

        /// <summary>
        /// Gets the type of the connector.
        /// </summary>
        public string ConnectorTypeId { get; init; }

        /// <summary>
        /// Gets the connector status.
        /// </summary>
        public virtual ConnectorStatus ConnectorStatus { get; init; }

        /// <summary>
        /// Gets the connector type.
        /// </summary>
        public virtual ConnectorType ConnectorType { get; init; }

        /// <summary>
        /// Gets the Customer Instance associated with the connector.
        /// </summary>
        public virtual CustomerInstance CustomerInstance { get; init; }

        /// <summary>
        /// Gets the connector instances associated with the connector type.
        /// </summary>
        public virtual ICollection<BuildingConnector> BuildingConnectors { get; init; }
    }
}

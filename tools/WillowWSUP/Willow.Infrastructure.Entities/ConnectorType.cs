#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents a connector type instance.
    /// </summary>
    public class ConnectorType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorType"/> class.
        /// </summary>
        public ConnectorType()
        {
            Connectors = new HashSet<Connector>();
        }

        /// <summary>
        /// Gets the connector type id
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the connector type name
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the connector type version
        /// </summary>
        public string Version { get; init; }

        /// <summary>
        /// Gets the direction of the connector.
        /// </summary>
        public string Direction { get; init; }

        /// <summary>
        /// Gets the connector instances associated with the connector type.
        /// </summary>
        public virtual ICollection<Connector> Connectors { get; init; }
    }
}

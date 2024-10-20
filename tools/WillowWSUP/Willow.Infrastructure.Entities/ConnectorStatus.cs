#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents a status of a connector
    /// </summary>
    public class ConnectorStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorStatus"/> class.
        /// </summary>
        public ConnectorStatus()
        {
            Connectors = new HashSet<Connector>();
        }

        /// <summary>
        /// Gets the identifier of the connector status.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the name of the connector status.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the description of the connector status.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets the connectors associated with the connectors status.
        /// </summary>
        public virtual ICollection<Connector> Connectors { get; init; }
    }
}

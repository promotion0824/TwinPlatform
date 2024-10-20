#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents a building connector instance.
    /// </summary>
    public class BuildingConnector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingConnector"/> class.
        /// </summary>
        public BuildingConnector()
        {
        }

        /// <summary>
        /// Gets the identifier of the building.
        /// </summary>
        public string BuildingId { get; init; }

        /// <summary>
        /// Gets the connector id of the connector.
        /// </summary>
        public string ConnectorId { get; init; }

        /// <summary>
        /// Gets the customer instance id of the connector.
        /// </summary>
        public Guid CustomerInstanceId { get; init; }

        /// <summary>
        /// Gets the status of the instance of the connector.
        /// </summary>
        public int BuildingConnectorStatusId { get; init; }

        /// <summary>
        /// Gets the building.
        /// </summary>
        public virtual Building Building { get; init; }

        /// <summary>
        /// Gets the connector.
        /// </summary>
        public virtual Connector Connector { get; init; }

        /// <summary>
        /// Gets the building connector status.
        /// </summary>
        public virtual BuildingConnectorStatus BuildingConnectorStatus { get; init; }
    }
}

#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents a status of a connector deployed to a building
    /// </summary>
    public class BuildingConnectorStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingConnectorStatus"/> class.
        /// </summary>
        public BuildingConnectorStatus()
        {
            BuildingConnectors = new HashSet<BuildingConnector>();
        }

        /// <summary>
        /// Gets the identifier of the building connector status.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the name of the building connector status.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the description of the building connector status.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets the building connectors associated with the building connectors status.
        /// </summary>
        public virtual ICollection<BuildingConnector> BuildingConnectors { get; init; }
    }
}

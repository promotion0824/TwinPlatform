#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents a customer instance.
    /// </summary>
    public class Building
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Building"/> class.
        /// </summary>
        public Building()
        {
            BuildingConnectors = new HashSet<BuildingConnector>();
        }

        /// <summary>
        /// Gets the twin identifier of the building.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the identifier of the customer instance.
        /// </summary>
        public Guid CustomerInstanceId { get; init; }

        /// <summary>
        /// Gets the name of the building.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the customer instance associated with the building.
        /// </summary>
        public virtual CustomerInstance CustomerInstance { get; init; }

        /// <summary>
        /// Gets the connectors associated with the building.
        /// </summary>
        public virtual ICollection<BuildingConnector> BuildingConnectors { get; init; }
    }
}

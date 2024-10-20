namespace Willow.TopologyIngestion.Entities
{
    /// <summary>
    /// The entity types to include in the topology.
    /// </summary>
    public class EntityTypes
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include the accounts.
        /// </summary>
        public bool IncludeAccounts { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to include the organization.
        /// </summary>
        public bool IncludeOrganization { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to include the sites.
        /// </summary>
        public bool IncludeSites { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to include the buildings.
        /// </summary>
        public bool IncludeBuildings { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to include the levels.
        /// </summary>
        public bool IncludeLevels { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to include the spaces.
        /// </summary>
        public bool IncludeSpaces { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to include the connectors.
        /// </summary>
        public bool IncludeConnectors { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to include the things.
        /// </summary>
        public bool IncludeThings { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to include the points.
        /// </summary>
        public bool IncludePoints { get; set; } = false;
    }
}

namespace Willow.TopologyIngestion.Mapped
{
    using DTDLParser;

    /// <summary>
    /// A list of relationship types used in the topology graph.
    /// </summary>
    internal class TwinMapping
    {
        /// <summary>
        /// Gets or sets the Mapped Identity for the twin.
        /// </summary>
        internal string MappedId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Willow Id for the twin.
        /// </summary>
        internal string WillowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Dtmi for the twin.
        /// </summary>
        internal Dtmi? Dtmi { get; set; } = null;

        /// <summary>
        /// Gets the Willow Identity for the twin.
        /// </summary>
        internal string TwinId
        {
            get
            {
                return string.IsNullOrWhiteSpace(WillowId) ? MappedId : WillowId;
            }
        }
    }
}

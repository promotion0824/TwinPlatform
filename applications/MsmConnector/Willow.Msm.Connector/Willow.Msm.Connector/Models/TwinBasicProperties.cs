namespace Willow.Msm.Connector.Models
{
    /// <summary>
    /// Represents the basic properties of a digital twin, providing core identifiers and descriptive information.
    /// </summary>
    public class TwinBasicProperties
    {
        /// <summary>
        /// Gets or sets the model identifier for the twin. This ID links the twin to its definition in the system's model registry.
        /// </summary>
        public string? ModelId { get; set; }

        /// <summary>
        /// Gets or sets the human-readable name of the twin. This name is typically used for display purposes and may be easier to recognize than the model ID.
        /// </summary>
        public string? Name { get; set; }
    }
}

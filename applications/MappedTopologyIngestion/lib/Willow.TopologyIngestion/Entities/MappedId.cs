namespace Willow.TopologyIngestion.Entities
{
    /// <summary>
    /// A class that represents the various mapped identities for a twin.
    /// </summary>
    public class MappedId
    {
        /// <summary>
        /// Gets or sets the exact type of the MappedId.
        /// </summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public string exactType { get; set; } = string.Empty;
#pragma warning restore SA1300 // Element should begin with upper-case letter

        /// <summary>
        /// Gets or sets the scope of the MappedId.
        /// </summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public string scope { get; set; } = string.Empty;
#pragma warning restore SA1300 // Element should begin with upper-case letter

        /// <summary>
        /// Gets or sets the scope id of the MappedId.
        /// </summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public string scopeId { get; set; } = string.Empty;
#pragma warning restore SA1300 // Element should begin with upper-case letter

        /// <summary>
        /// Gets or sets the value of the MappedId.
        /// </summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public string value { get; set; } = string.Empty;
#pragma warning restore SA1300 // Element should begin with upper-case letter

        /// <summary>
        /// Gets or sets the dateCreated of the MappedId.
        /// </summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public string dateCreated { get; set; } = string.Empty;
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}

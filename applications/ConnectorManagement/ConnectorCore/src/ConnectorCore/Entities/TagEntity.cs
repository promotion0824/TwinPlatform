namespace ConnectorCore.Entities
{
    using System;

    /// <summary>
    /// Represents a tag entity.
    /// </summary>
    public class TagEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the tag entity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the tag.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the client ID associated with the tag.
        /// </summary>
        public Guid? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the feature of the tag.
        /// </summary>
        public string Feature { get; set; }
    }
}

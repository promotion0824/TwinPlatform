namespace ConnectorCore.Models
{
    using System;

    /// <summary>
    /// Represents a link between a point and a tag.
    /// </summary>
    public class PointToTagLink
    {
        /// <summary>
        /// Gets or sets the point ID.
        /// </summary>
        public Guid PointId { get; set; }

        /// <summary>
        /// Gets or sets the tag ID.
        /// </summary>
        public Guid TagId { get; set; }
    }
}

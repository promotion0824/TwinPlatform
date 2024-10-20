namespace ConnectorCore.Models
{
    using System;

    /// <summary>
    /// Represents a link between equipment and a tag.
    /// </summary>
    public class EquipmentToTagLink
    {
        /// <summary>
        /// Gets or sets the equipment ID.
        /// </summary>
        public Guid EquipmentId { get; set; }

        /// <summary>
        /// Gets or sets the tag ID.
        /// </summary>
        public Guid TagId { get; set; }
    }
}

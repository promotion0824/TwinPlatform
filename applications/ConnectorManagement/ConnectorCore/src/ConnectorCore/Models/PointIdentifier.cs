namespace ConnectorCore.Models
{
    using System;

    /// <summary>
    /// Represents an identifier for a point.
    /// </summary>
    public class PointIdentifier
    {
        /// <summary>
        /// Gets or sets the point entity ID.
        /// </summary>
        public Guid PointEntityId { get; set; }

        /// <summary>
        /// Gets or sets the equipment ID.
        /// </summary>
        public Guid? EquipmentId { get; set; }
    }
}

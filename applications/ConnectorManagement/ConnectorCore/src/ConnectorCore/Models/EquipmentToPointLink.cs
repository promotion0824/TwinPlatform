namespace ConnectorCore.Models
{
    using System;

    /// <summary>
    /// Represents a link between equipment and a point.
    /// </summary>
    public class EquipmentToPointLink
    {
        /// <summary>
        /// Gets or sets the equipment ID.
        /// </summary>
        public Guid EquipmentId { get; set; }

        /// <summary>
        /// Gets or sets the point entity ID.
        /// </summary>
        public Guid PointEntityId { get; set; }
    }
}

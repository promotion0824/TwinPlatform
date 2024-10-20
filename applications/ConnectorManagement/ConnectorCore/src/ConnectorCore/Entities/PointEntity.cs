namespace ConnectorCore.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a point entity.
    /// </summary>
    public class PointEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the point entity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the entity ID associated with the point.
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the name of the point.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the client ID associated with the point.
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Gets or sets the site ID associated with the point.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets or sets the unit of the point.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the type of the point.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Gets or sets the external point ID.
        /// </summary>
        public string ExternalPointId { get; set; }

        /// <summary>
        /// Gets or sets the category of the point.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the metadata for the point.
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the point is detected.
        /// </summary>
        public bool IsDetected { get; set; }

        /// <summary>
        /// Gets or sets the device ID associated with the point.
        /// </summary>
        public Guid DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the device entity associated with the point.
        /// </summary>
        [NotMapped]
        public DeviceEntity Device { get; set; }

        /// <summary>
        /// Gets or sets the collection of tags associated with the point.
        /// </summary>
        [NotMapped]
        public IList<TagEntity> Tags { get; set; }

        /// <summary>
        /// Gets or sets the collection of equipment associated with the point.
        /// </summary>
        [NotMapped]
        public IList<EquipmentEntity> Equipment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the point is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}

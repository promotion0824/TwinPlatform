namespace ConnectorCore.Entities
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an equipment entity.
    /// </summary>
    public class EquipmentEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the equipment entity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the equipment.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the client ID associated with the equipment.
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Gets or sets the site ID associated with the equipment.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets or sets the floor ID associated with the equipment.
        /// </summary>
        public Guid? FloorId { get; set; }

        /// <summary>
        /// Gets or sets the external equipment ID.
        /// </summary>
        public string ExternalEquipmentId { get; set; }

        /// <summary>
        /// Gets or sets the category of the equipment.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the parent ID of the equipment.
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the collection of points associated with the equipment.
        /// </summary>
        public IList<PointEntity> Points { get; set; } = new List<PointEntity>();

        /// <summary>
        /// Gets or sets the collection of categories associated with the equipment.
        /// </summary>
        public IList<CategoryEntity> Categories { get; set; } = new List<CategoryEntity>();

        /// <summary>
        /// Gets or sets the collection of tags associated with the equipment.
        /// </summary>
        public IList<TagEntity> Tags { get; set; } = new List<TagEntity>();

        /// <summary>
        /// Gets or sets the collection of point tags associated with the equipment.
        /// </summary>
        public IList<TagEntity> PointTags { get; set; } = new List<TagEntity>();
    }
}

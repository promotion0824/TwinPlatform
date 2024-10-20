namespace ConnectorCore.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConnectorCore.Entities;

    /// <summary>
    /// Represents a data transfer object for equipment.
    /// </summary>
    public class EquipmentDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the equipment.
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
        /// Gets or sets the parent equipment ID.
        /// </summary>
        public Guid? ParentEquipmentId { get; set; }

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
        public IList<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the collection of point tags associated with the equipment.
        /// </summary>
        public IList<string> PointTags { get; set; } = new List<string>();

        /// <summary>
        /// Creates an instance of <see cref="EquipmentDto"/> from an <see cref="EquipmentEntity"/>.
        /// </summary>
        /// <param name="entity">The equipment entity.</param>
        /// <returns>The created equipment DTO.</returns>
        public static EquipmentDto Create(EquipmentEntity entity)
        {
            var result = new EquipmentDto
            {
                Category = entity.Category,
                ClientId = entity.ClientId,
                ExternalEquipmentId = entity.ExternalEquipmentId,
                FloorId = entity.FloorId,
                Id = entity.Id,
                Name = entity.Name,
                ParentEquipmentId = entity.ParentId,
                Points = entity.Points,
                SiteId = entity.SiteId,
                PointTags = entity.PointTags.Select(q => q.Name).ToList(),
                Tags = entity.Tags.Select(q => q.Name).ToList(),
            };

            return result;
        }

        /// <summary>
        /// Creates a list of <see cref="EquipmentDto"/> from a collection of <see cref="EquipmentEntity"/>.
        /// </summary>
        /// <param name="entities">The collection of equipment entities.</param>
        /// <returns>The list of created equipment DTOs.</returns>
        public static List<EquipmentDto> Create(IEnumerable<EquipmentEntity> entities)
        {
            return entities.Select(Create).ToList();
        }
    }
}

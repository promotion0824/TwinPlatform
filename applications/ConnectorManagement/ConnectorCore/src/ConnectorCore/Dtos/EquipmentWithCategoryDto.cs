namespace ConnectorCore.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConnectorCore.Entities;

    /// <summary>
    /// Represents a data transfer object for equipment with categories.
    /// </summary>
    public class EquipmentWithCategoryDto
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
        /// Gets or sets the floor ID associated with the equipment.
        /// </summary>
        public Guid? FloorId { get; set; }

        /// <summary>
        /// Gets or sets the list of tags associated with the equipment.
        /// </summary>
        public List<TagEntity> Tags { get; set; }

        /// <summary>
        /// Gets or sets the list of category IDs associated with the equipment.
        /// </summary>
        public List<Guid> CategoryIds { get; set; }

        /// <summary>
        /// Gets or sets the list of point tags associated with the equipment.
        /// </summary>
        public List<TagDto> PointTags { get; set; }

        /// <summary>
        /// Maps an <see cref="EquipmentEntity"/> to an <see cref="EquipmentWithCategoryDto"/>.
        /// </summary>
        /// <param name="entity">The equipment entity.</param>
        /// <returns>The mapped equipment DTO.</returns>
        public static EquipmentWithCategoryDto Map(EquipmentEntity entity)
        {
            var dto = new EquipmentWithCategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                FloorId = entity.FloorId,
                Tags = entity.Tags.ToList(),
                CategoryIds = entity.Categories.Select(c => c.Id).ToList(),
                PointTags = TagDto.Map(entity.PointTags),
            };
            return dto;
        }

        /// <summary>
        /// Maps a collection of <see cref="EquipmentEntity"/> to a list of <see cref="EquipmentWithCategoryDto"/>.
        /// </summary>
        /// <param name="entities">The collection of equipment entities.</param>
        /// <returns>The list of mapped equipment DTOs.</returns>
        public static List<EquipmentWithCategoryDto> Map(IEnumerable<EquipmentEntity> entities)
        {
            return entities.Select(Map).ToList();
        }
    }
}

namespace ConnectorCore.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConnectorCore.Entities;

    /// <summary>
    /// Represents a simplified data transfer object for equipment.
    /// </summary>
    public class EquipmentSimpleDto
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
        /// Maps an <see cref="EquipmentEntity"/> to an <see cref="EquipmentSimpleDto"/>.
        /// </summary>
        /// <param name="entity">The equipment entity.</param>
        /// <returns>The mapped equipment DTO.</returns>
        public static EquipmentSimpleDto Map(EquipmentEntity entity)
        {
            var dto = new EquipmentSimpleDto
            {
                Id = entity.Id,
                Name = entity.Name,
                FloorId = entity.FloorId,
            };
            return dto;
        }

        /// <summary>
        /// Maps a collection of <see cref="EquipmentEntity"/> to a list of <see cref="EquipmentSimpleDto"/>.
        /// </summary>
        /// <param name="entities">The collection of equipment entities.</param>
        /// <returns>The list of mapped equipment DTOs.</returns>
        public static List<EquipmentSimpleDto> Map(IEnumerable<EquipmentEntity> entities)
        {
            return entities.Select(Map).ToList();
        }
    }
}

namespace ConnectorCore.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using ConnectorCore.Entities;

    /// <summary>
    /// Represents a data transfer object for a point.
    /// </summary>
    public class PointDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the point.
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
        /// Gets or sets the metadata of the point.
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
        /// Gets or sets the device associated with the point.
        /// </summary>
        [NotMapped]
        public DeviceEntity Device { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the point.
        /// </summary>
        [NotMapped]
        public IList<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the equipment associated with the point.
        /// </summary>
        [NotMapped]
        public IList<EquipmentEntity> Equipment { get; set; } = new List<EquipmentEntity>();

        /// <summary>
        /// Gets or sets a value indicating whether the point is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="PointDto"/> from a <see cref="PointEntity"/>.
        /// </summary>
        /// <param name="entity">The point entity.</param>
        /// <returns>The created point DTO.</returns>
        public static PointDto Create(PointEntity entity)
        {
            var result = new PointDto
            {
                Category = entity.Category,
                ClientId = entity.ClientId,
                Device = entity.Device,
                DeviceId = entity.DeviceId,
                EntityId = entity.EntityId,
                Equipment = entity.Equipment,
                ExternalPointId = entity.ExternalPointId,
                Id = entity.Id,
                IsDetected = entity.IsDetected,
                IsEnabled = entity.IsEnabled,
                Metadata = entity.Metadata,
                Name = entity.Name,
                SiteId = entity.SiteId,
                Type = entity.Type,
                Unit = entity.Unit,
                Tags = entity.Tags?.Select(q => q.Name).ToList(),
            };

            return result;
        }

        /// <summary>
        /// Creates a list of <see cref="PointDto"/> from a collection of <see cref="PointEntity"/>.
        /// </summary>
        /// <param name="entities">The collection of point entities.</param>
        /// <returns>The list of created point DTOs.</returns>
        public static List<PointDto> Create(IEnumerable<PointEntity> entities)
        {
            return entities.Select(Create).ToList();
        }
    }
}

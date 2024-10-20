namespace ConnectorCore.Dtos
{
    using System.Collections.Generic;
    using System.Linq;
    using ConnectorCore.Entities;

    /// <summary>
    /// Represents a data transfer object for a tag.
    /// </summary>
    public class TagDto
    {
        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the feature of the tag.
        /// </summary>
        public string Feature { get; set; }

        /// <summary>
        /// Maps a <see cref="TagEntity"/> to a <see cref="TagDto"/>.
        /// </summary>
        /// <param name="entity">The tag entity.</param>
        /// <returns>The mapped tag DTO.</returns>
        public static TagDto Map(TagEntity entity)
        {
            var dto = new TagDto
            {
                Name = entity.Name,
                Feature = entity.Feature,
            };
            return dto;
        }

        /// <summary>
        /// Maps a collection of <see cref="TagEntity"/> to a list of <see cref="TagDto"/>.
        /// </summary>
        /// <param name="entities">The collection of tag entities.</param>
        /// <returns>The list of mapped tag DTOs.</returns>
        public static List<TagDto> Map(IEnumerable<TagEntity> entities)
        {
            return entities.Select(Map).ToList();
        }
    }
}

namespace ConnectorCore.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a category entity.
    /// </summary>
    public class CategoryEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the category.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the site ID associated with the category.
        /// </summary>
        public Guid? SiteId { get; set; }

        /// <summary>
        /// Gets or sets the client ID associated with the category.
        /// </summary>
        public Guid? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the parent ID of the category.
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the category has children.
        /// </summary>
        public bool HasChildren { get; set; }

        /// <summary>
        /// Gets or sets the collection of tags associated with the category.
        /// </summary>
        [NotMapped]
        public List<TagEntity> Tags { get; set; } = new List<TagEntity>();
    }
}

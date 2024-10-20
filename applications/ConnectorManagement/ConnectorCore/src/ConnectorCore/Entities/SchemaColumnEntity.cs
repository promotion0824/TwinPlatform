namespace ConnectorCore.Entities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a schema column entity.
    /// </summary>
    [DisplayName("SchemaColumn")]
    [Table("SchemaColumn")]
    public class SchemaColumnEntity
    {
        /// <summary>
        /// Gets or sets id of the column.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets name of the schema column.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is schema column required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets data type of the schema column.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets iD of schema to column belongs to.
        /// </summary>
        public Guid SchemaId { get; set; }

        /// <summary>
        /// Gets or sets iD of schema to column belongs to.
        /// </summary>
        public string UnitOfMeasure { get; set; }
    }
}

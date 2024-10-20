namespace ConnectorCore.Entities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    [DisplayName("Schema")]
    [Table("Schema")]
    internal class SchemaEntity
    {
        /// <summary>
        /// Gets or sets id of the schema.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets name of the schema.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets schema's type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets client id the schema belongs to.
        /// </summary>
        public Guid? ClientId { get; set; }
    }
}

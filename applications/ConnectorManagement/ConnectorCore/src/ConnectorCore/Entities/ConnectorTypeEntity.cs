namespace ConnectorCore.Entities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ConnectorType")]
    [DisplayName("ConnectorType")]
    internal class ConnectorTypeEntity
    {
        /// <summary>
        /// Gets or sets type id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets type name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets id of connector's configuration schema.
        /// </summary>
        public Guid ConnectorConfigurationSchemaId { get; set; }

        //Id of device's metadata schema
        public Guid DeviceMetadataSchemaId { get; set; }

        /// <summary>
        /// Gets or sets id of point metadata schema.
        /// </summary>
        public Guid PointMetadataSchemaId { get; set; }

        /// <summary>
        /// Gets or sets id of scan's configuration schema.
        /// </summary>
        public Guid? ScanConfigurationSchemaId { get; set; }
    }
}

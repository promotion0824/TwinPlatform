namespace ConnectorCore.Models
{
    using System;

    /// <summary>
    /// Represents the data required for connector validation.
    /// </summary>
    public class ConnectorDataForValidation
    {
        /// <summary>
        /// Gets or sets the site ID associated with the connector.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets or sets the device metadata schema ID.
        /// </summary>
        public Guid DeviceMetadataSchemaId { get; set; }

        /// <summary>
        /// Gets or sets the point metadata schema ID.
        /// </summary>
        public Guid PointMetadataSchemaId { get; set; }
    }
}

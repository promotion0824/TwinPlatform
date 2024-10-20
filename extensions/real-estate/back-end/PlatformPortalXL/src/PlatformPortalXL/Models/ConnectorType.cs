using System;

namespace PlatformPortalXL.Models
{
    public class ConnectorType
    {
        /// <summary>
        /// Type id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Type name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Id of connector's configuration schema
        /// </summary>
        public Guid ConnectorConfigurationSchemaId { get; set; }

        //Id of device's metadata schema
        public Guid DeviceMetadataSchemaId { get; set; }

        /// <summary>
        /// Id of point metadata schema
        /// </summary>
        public Guid PointMetadataSchemaId { get; set; }

        /// <summary>
        /// Id of connector's category
        /// </summary>
        public Guid ConnectorCategoryId { get; set; }

        /// <summary>
        /// Id of scan's configuration schema
        /// </summary>
        public Guid? ScanConfigurationSchemaId { get; set; }
    }
}

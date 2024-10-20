namespace ConnectorCore.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using ConnectorCore.Entities;

    /// <summary>
    /// Represents a data transfer object for connector import validation.
    /// </summary>
    public class ConnectorForImportValidationDto
    {
        /// <summary>
        /// Gets or sets all tag names.
        /// </summary>
        public List<string> AllTagNames { get; set; }

        /// <summary>
        /// Gets or sets the site ID associated with the connector.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets or sets the device schema columns.
        /// </summary>
        public IList<SchemaColumnEntity> DeviceSchemaColumns { get; set; }

        /// <summary>
        /// Gets or sets the point schema columns.
        /// </summary>
        public IList<SchemaColumnEntity> PointSchemaColumns { get; set; }

        /// <summary>
        /// Gets or sets all point types.
        /// </summary>
        public IList<int> AllPointTypes { get; set; }

        /// <summary>
        /// Gets or sets all external points for the site excluding the connector.
        /// </summary>
        public IList<string> AllExternalPointsForSiteExcludingConnector { get; set; }

        /// <summary>
        /// Gets or sets the entity ID by point ID.
        /// </summary>
        [JsonIgnore]
        public Dictionary<Guid, Guid> EntityIdByPointId { get; set; }

        /// <summary>
        /// Gets or sets the entity ID by point ID for serialization.
        /// </summary>
        /// <remarks>
        /// System.Text.Json doesn't support non-string key serialization, so need to manually convert non-string to string key.
        /// </remarks>
        [JsonPropertyName("entityIdByPointId")]
        public Dictionary<string, Guid> EntityIdByPointIdForSerialization
        {
            get
            {
                return EntityIdByPointId.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
            }

            set
            {
                EntityIdByPointId = EntityIdByPointIdForSerialization
                    .ToDictionary(kvp => Guid.Parse(kvp.Key), kvp => kvp.Value);
            }
        }

        /// <summary>
        /// Gets or sets all device IDs.
        /// </summary>
        public IList<Guid> AllDeviceIds { get; set; }

        /// <summary>
        /// Gets or sets all equipment IDs.
        /// </summary>
        public IList<Guid> AllEquipmentIds { get; set; }
    }
}

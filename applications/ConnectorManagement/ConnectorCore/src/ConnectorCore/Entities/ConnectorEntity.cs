namespace ConnectorCore.Entities
{
    using System;

    /// <summary>
    /// Represents a connector entity.
    /// </summary>
    public class ConnectorEntity
    {
        /// <summary>
        /// Gets or sets id of the connector.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets connector's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the client ID associated with the connector.
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Gets or sets the site ID associated with the connector.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets or sets the configuration of the connector.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets id of connector's type.
        /// </summary>
        public Guid ConnectorTypeId { get; set; }

        /// <summary>
        /// Gets or sets error threshold.
        /// </summary>
        public int ErrorThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether flag marking enabled connectors.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether flag marking if logging is enabled for the connector.
        /// </summary>
        public bool IsLoggingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the registration ID for the connector.
        /// </summary>
        public string RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the registration key for the connector.
        /// </summary>
        public string RegistrationKey { get; set; }

        /// <summary>
        /// Gets or sets the last import date and time for the connector.
        /// </summary>
        public DateTime? LastImport { get; set; }

        /// <summary>
        /// Gets or sets the last updated date and time for the connector.
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the connection type of the connector.
        /// </summary>
        public string ConnectionType { get; set; }

        /// <summary>
        /// Gets or sets the points count for the connector.
        /// </summary>
        public int PointsCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the connector is archived.
        /// </summary>
        public bool IsArchived { get; set; }
    }

    internal static class ConnectorExtensions
    {
        public static ConnectorEntity ToConnectorEntity(this ConnectorCore.Data.Models.Connector connector)
        {
            return new ConnectorEntity
            {
                Id = connector.Id,
                Name = connector.Name,
                ClientId = connector.ClientId,
                SiteId = connector.SiteId,
                Configuration = connector.Configuration,
                ConnectorTypeId = connector.ConnectorTypeId,
                ErrorThreshold = connector.ErrorThreshold,
                IsEnabled = connector.IsEnabled,
                IsLoggingEnabled = connector.IsLoggingEnabled,
                RegistrationId = connector.RegistrationId,
                RegistrationKey = connector.RegistrationKey,
                LastImport = connector.LastImport,
                LastUpdatedAt = connector.LastUpdatedAt,
                ConnectionType = connector.ConnectionType,
                IsArchived = Convert.ToBoolean(connector.IsArchived),
            };
        }

        public static Data.Models.Connector ToConnector(this ConnectorEntity connectorEntity)
        {
            return new Data.Models.Connector
            {
                Id = connectorEntity.Id,
                Name = connectorEntity.Name,
                ClientId = connectorEntity.ClientId,
                SiteId = connectorEntity.SiteId,
                Configuration = connectorEntity.Configuration,
                ConnectorTypeId = connectorEntity.ConnectorTypeId,
                ErrorThreshold = connectorEntity.ErrorThreshold,
                IsEnabled = connectorEntity.IsEnabled,
                IsLoggingEnabled = connectorEntity.IsLoggingEnabled,
                RegistrationId = connectorEntity.RegistrationId,
                RegistrationKey = connectorEntity.RegistrationKey,
                LastImport = connectorEntity.LastImport,
                LastUpdatedAt = connectorEntity.LastUpdatedAt,
                ConnectionType = connectorEntity.ConnectionType,
                IsArchived = connectorEntity.IsArchived,
            };
        }
    }
}

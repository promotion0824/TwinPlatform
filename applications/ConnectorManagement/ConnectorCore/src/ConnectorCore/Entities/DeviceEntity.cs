namespace ConnectorCore.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a device entity.
    /// </summary>
    public class DeviceEntity
    {
        /// <summary>
        /// Gets or sets device's ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets device's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the client ID associated with the device.
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Gets or sets the site ID associated with the device.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets or sets the external device ID.
        /// </summary>
        public string ExternalDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the registration ID.
        /// </summary>
        public string RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the registration key.
        /// </summary>
        public string RegistrationKey { get; set; }

        /// <summary>
        /// Gets or sets the metadata of the device.
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the device is detected.
        /// </summary>
        public bool IsDetected { get; set; }

        /// <summary>
        /// Gets or sets the connector ID associated with the device.
        /// </summary>
        public Guid ConnectorId { get; set; }

        /// <summary>
        /// Gets or sets the points associated with the device.
        /// </summary>
        [NotMapped]
        public IList<PointEntity> Points { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the device is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the connector associated with the device.
        /// </summary>
        [NotMapped]
        public ConnectorEntity Connector { get; set; }
    }
}

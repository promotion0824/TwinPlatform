namespace ConnectorCore.Dtos
{
#nullable enable
    using System;

    /// <summary>
    /// Represents the state of a connector.
    /// </summary>
    public class ConnectorStateDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the connector.
        /// </summary>
        public Guid ConnectorId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the connector state.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the connector is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the connector is archived.
        /// </summary>
        public bool Archived { get; set; }

        /// <summary>
        /// Gets or sets the connection type of the connector.
        /// </summary>
        public string? ConnectionType { get; set; }

        /// <summary>
        /// Gets or sets the interval of the connector state.
        /// </summary>
        public int Interval { get; set; }
    }
}

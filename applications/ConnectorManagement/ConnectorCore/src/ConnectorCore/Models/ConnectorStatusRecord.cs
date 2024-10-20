namespace ConnectorCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using ConnectorCore.Common.Extensions;

    /// <summary>
    /// Represents a record of the connector status.
    /// </summary>
    public class ConnectorStatusRecord
    {
        /// <summary>
        /// Gets or sets the unique identifier for the connector.
        /// </summary>
        public Guid ConnectorId { get; set; }

        /// <summary>
        /// Gets or sets the site ID associated with the connector.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets or sets the client ID associated with the connector.
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Gets or sets the name of the connector.
        /// </summary>
        public string ConnectorName { get; set; }

        /// <summary>
        /// Gets or sets the source statuses of the connector.
        /// </summary>
        public List<ConnectorSourceStatusRecord> SourceStatuses { get; set; } = new List<ConnectorSourceStatusRecord>();

        /// <summary>
        /// Gets the overall status of the connector.
        /// </summary>
        public ConnectorStatus OverallStatus
        {
            get
            {
                if (!SourceStatuses.Any())
                {
                    return ConnectorStatus.Offline;
                }

                if (SourceStatuses.All(s => s.Status == ConnectorStatus.Offline))
                {
                    return ConnectorStatus.Offline;
                }

                if (SourceStatuses.All(s => s.Status == ConnectorStatus.Online))
                {
                    return ConnectorStatus.Online;
                }

                return ConnectorStatus.OnlineWithErrors;
            }
        }

        /// <summary>
        /// Gets the overall status description of the connector.
        /// </summary>
        public string OverallStatusDescription => OverallStatus.GetDisplayName();

        /// <summary>
        /// Gets or sets the points count of the connector.
        /// </summary>
        public int? PointsCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the connector is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }
    }

    /// <summary>
    /// Represents a record of the connector source status.
    /// </summary>
    public class ConnectorSourceStatusRecord
    {
        /// <summary>
        /// Gets or sets the source of the connector.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the status of the connector source.
        /// </summary>
        public ConnectorStatus Status { get; set; }

        /// <summary>
        /// Gets the status description of the connector source.
        /// </summary>
        public string StatusDescription => Status.GetDisplayName();

        /// <summary>
        /// Gets or sets the error count of the connector source.
        /// </summary>
        public int? ErrorCount { get; set; }
    }
}

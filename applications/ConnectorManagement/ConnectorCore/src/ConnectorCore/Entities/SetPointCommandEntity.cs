namespace ConnectorCore.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents a set point command entity.
    /// </summary>
    public class SetPointCommandEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the set point command.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the site ID associated with the set point command.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets or sets the connector ID associated with the set point command.
        /// </summary>
        public Guid ConnectorId { get; set; }

        /// <summary>
        /// Gets or sets the equipment ID associated with the set point command.
        /// </summary>
        public Guid EquipmentId { get; set; }

        /// <summary>
        /// Gets or sets the insight ID associated with the set point command.
        /// </summary>
        public Guid InsightId { get; set; }

        /// <summary>
        /// Gets or sets the point ID associated with the set point command.
        /// </summary>
        public Guid PointId { get; set; }

        /// <summary>
        /// Gets or sets the set point ID associated with the set point command.
        /// </summary>
        public Guid SetPointId { get; set; }

        /// <summary>
        /// Gets or sets the current reading of the set point command.
        /// </summary>
        public decimal CurrentReading { get; set; }

        /// <summary>
        /// Gets or sets the original value of the set point command.
        /// </summary>
        public decimal OriginalValue { get; set; }

        /// <summary>
        /// Gets or sets the desired value of the set point command.
        /// </summary>
        public decimal DesiredValue { get; set; }

        /// <summary>
        /// Gets or sets the desired duration in minutes for the set point command.
        /// </summary>
        public int DesiredDurationMinutes { get; set; }

        /// <summary>
        /// Gets or sets the status of the set point command.
        /// </summary>
        public SetPointCommandStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the type of the set point command.
        /// </summary>
        public SetPointCommandType Type { get; set; }

        /// <summary>
        /// Gets or sets the unit of the set point command.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the creation date and time of the set point command.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last updated date and time of the set point command.
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the error description of the set point command.
        /// </summary>
        [StringLength(512)]
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created the set point command.
        /// </summary>
        public Guid? CreatedBy { get; set; }
    }
}

namespace ConnectorCore.Entities
{
    /// <summary>
    /// Represents a set point command configuration entity.
    /// </summary>
    public class SetPointCommandConfigurationEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the set point command configuration.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the set point command.
        /// </summary>
        public SetPointCommandType Type { get; set; }

        /// <summary>
        /// Gets or sets the description of the set point command.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the insight name associated with the set point command.
        /// </summary>
        public string InsightName { get; set; }

        /// <summary>
        /// Gets or sets the point tags associated with the set point command.
        /// </summary>
        public string PointTags { get; set; }

        /// <summary>
        /// Gets or sets the set point tags associated with the set point command.
        /// </summary>
        public string SetPointTags { get; set; }

        /// <summary>
        /// Gets or sets the desired value limitation for the set point command.
        /// </summary>
        public decimal DesiredValueLimitation { get; set; }
    }
}

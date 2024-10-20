namespace Willow.Msm.Connector.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The type of energy provided.
    /// Vaues for Msdyn_energytype. See: https://learn.microsoft.com/en-us/common-data-model/schema/core/industrycommon/sustainability/sustainabilityenergy/generatedenergy#energytype.
    /// </summary>
    /// <remarks>
    /// The Microsoft documetation for MSM is under development. The link provided is a placeholder as we are getting most of our information from the Microsft Engineering Team.
    /// </remarks>
    public enum EnergyType
    {
        /// <summary>
        /// Electricity.
        /// Sustainability Data Definition "Purchased Electricity" 35c356cc-3edb-44aa-8243-9ad44f4d6ed4.
        /// </summary>
        Electricity = 700610000,
    }

    /// <summary>
    /// The quality of the data.
    /// Values for Msdyn_dataqualitytype. See: https://learn.microsoft.com/en-us/common-data-model/schema/core/industrycommon/sustainability/sustainabilityenergy/generatedenergy#dataqualitytype.
    /// </summary>
    /// <remarks>
    /// The Microsoft documetation for MSM is under development. The link provided is a placeholder as we are getting most of our information from the Microsft Engineering Team.
    /// </remarks>
    public enum DataQualityType
    {
        /// <summary>
        /// The actual energy consumption data.
        /// </summary>
        Actual = 700610000,

        /// <summary>
        /// The estimated energy consumption data.
        /// </summary>
        Estimated = 700610001,

        /// <summary>
        /// The metered energy consumption data.
        /// </summary>
        Metered = 700610002,
    }

    /// <summary>
    /// Represents detailed information about a purchased energy twin, including identifiers, names,
    /// and associated metadata necessary for managing energy data within a digital twin model.
    /// </summary>
    public class PurchasedEnergyTwinDetail
    {
        /// <summary>
        /// Gets or sets the model identifier associated with the purchased energy twin.
        /// </summary>
        [Required]
        public required string ModelId { get; set; }

        /// <summary>
        /// Gets or sets the twin identifier for the purchased energy record.
        /// </summary>
        [Required]
        public required string TwinId { get; set; }

        /// <summary>
        /// Gets or sets the name of the purchased energy twin.
        /// </summary>
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the unit of measurement for the purchased energy data.
        /// </summary>
        [Required]
        public required string Unit { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the trend data associated with the energy consumption.
        /// </summary>
        [Required]
        public required string TrendId { get; set; }

        /// <summary>
        /// Gets or sets the list of timestamped quantities representing energy consumption data over time.
        /// </summary>
        [Required]
        public List<TimestampedDataPoint>? Quantities { get; set; }

        /// <summary>
        /// Gets or sets the data quality associated with the record.
        /// </summary>
        [Required]
        public required DataQualityType Msdyn_dataqualitytype { get; set; }

        /// <summary>
        /// Gets or sets the twin identifier of the meter hosting this purchased energy data, if applicable.
        /// </summary>
        public string? HostedByMeterTwinId { get; set; }

        /// <summary>
        /// Gets or sets the twin identifier of the utility account that this energy capability is associated with, if applicable.
        /// </summary>
        public string? IsCapabilityOfUtilityAccountTwinId { get; set; }

        /// <summary>
        /// Gets or sets the name of the energy provider.
        /// </summary>
        [Required]
        public string? Msdyn_energyprovidername { get; set; }

        /// <summary>
        /// Gets or sets the type of energy provided.
        /// </summary>
        [Required]
        public required EnergyType Msdyn_energytype { get; set; }

        /// <summary>
        /// Gets or sets the description of the purchased energy record.
        /// </summary>
        [Required]
        public required string Msdyn_description { get; set; }
    }
}

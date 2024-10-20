namespace Willow.Msm.Connector.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents a request message for retrieving carbon activity data.
    /// This includes specifications for the type of entity, aggregation level and relevant organizational details.
    /// </summary>
    public class CarbonActivityRequestMessage
    {
        /// <summary>
        /// Gets or sets the type of the Energy for which carbon activity data is requested.
        /// Only "Electricity" is supported at the current time.
        /// </summary>
        /// <remarks>
        /// Used to select the table into which the data will be inserted.
        /// https://learn.microsoft.com/en-us/common-data-model/schema/core/industrycommon/sustainability/sustainabilitycarbon/purchasedenergy#energytype.
        /// </remarks>
        [Required]
        [AllowedValues("Electricity")]
        public required string EnergyType { get; set; }

        /// <summary>
        /// Gets or sets the aggregation window for the data request.
        /// Allowed values are "None", "Day", "Week", "Month".
        /// </summary>
        [Required]
        [AllowedValues("None", "Day", "Week", "Month")]
        public required string AggregationWindow { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the organizational unit involved in the request.
        /// </summary>
        [Required]
        public required string OrganizationalUnitId { get; set; }

        /// <summary>
        /// Gets or sets the short name of the organization making the request.
        /// </summary>
        [Required]
        public required string OrganizationShortName { get; set; }

        /// <summary>
        /// Gets or sets the client identifier for authentication purposes.
        /// </summary>
        [Required]
        public required string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret for authentication purposes.
        /// </summary>
        [Required]
        public required string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the watermark date for the request, which is used to determine the range of data concerned or the starting point for data retrieval.
        /// </summary>
        [Required]
        public required DateTime WatermarkDate { get; set; }
    }
}

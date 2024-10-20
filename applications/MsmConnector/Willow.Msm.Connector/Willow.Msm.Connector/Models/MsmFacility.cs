namespace Willow.Msm.Connector.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Details of the Facility or Site.
    /// </summary>
    public class MsmFacility
    {
        /// <summary>
        /// Gets or sets unique identifier for the facility.
        /// </summary>
        [Required]
        public string Msdyn_facilityid { get; set; } = null!;

        /// <summary>
        /// Gets or sets unique identifier for the customer site.
        /// </summary>
        [Required]
        public string SiteId { get; set; } = null!;

        /// <summary>
        /// Gets or sets Details of the purchased energy.
        /// </summary>
        [Required]
        public List<PurchasedEnergyTwinDetail> PurchasedEnergyTwinDetails { get; set; } = null!;
    }
}

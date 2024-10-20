namespace Willow.Msm.Connector.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Top level Organization details for the MSM Model.
    /// </summary>
    public class MsmOrganization
    {
        /// <summary>
        /// Gets or sets the unique Identifier for the Organization.
        /// </summary>
        [Required]
        public string? Msdyn_organizationalunitid { get; set; }

        /// <summary>
        /// gets or sets the list of Facilities associated with the Organization.
        /// </summary>
        [Required]
        public List<MsmFacility>? MsmFacilities { get; set; }
    }
}

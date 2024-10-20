namespace Willow.Msm.Connector.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an authentication token used in Willow applications, encapsulating the token value and its expiry information.
    /// </summary>
    public class WillowToken
    {
        /// <summary>
        /// Gets or sets the token string used for authentication purposes.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the token expires and is no longer valid for use.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }
    }
}

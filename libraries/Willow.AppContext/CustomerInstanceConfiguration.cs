namespace Willow.AppContext
{
    /// <summary>
    /// Configuration information for the Customer Instance.
    /// </summary>
    public class CustomerInstanceConfiguration
    {
        /// <summary>
        /// Gets or sets the Unique Id of the Customer in the Sales System.
        /// </summary>
        public string CustomerSalesId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Name of the Customer Instance.
        /// </summary>
        public string CustomerInstanceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets a List of Key-Value Pairs of all configuration settings for the object needed for adding to metrics and logs as dimensions.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Values
        {
            get
            {
                var kvps = new List<KeyValuePair<string, object>>
                {
                    new("CustomerSalesId", CustomerSalesId.ToString()),
                    new("CustomerInstanceName", CustomerInstanceName),
                };
                return kvps;
            }
        }
    }
}

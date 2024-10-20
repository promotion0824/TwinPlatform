namespace Willow.AppContext
{
    /// <summary>
    /// Configuration information for the Region.
    /// </summary>
    public class RegionConfigurationOptions
    {
        /// <summary>
        /// Gets or sets the Short Name of the Region.
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Gets a List of Key-Value Pairs of all configuration settings for the object needed for adding to metrics and logs as dimensions.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Values
        {
            get
            {
                var kvps = new List<KeyValuePair<string, object>>
                {
                    new("RegionShortName", ShortName),
                };
                return kvps;
            }
        }
    }
}

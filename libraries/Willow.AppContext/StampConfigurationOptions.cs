namespace Willow.AppContext
{
    /// <summary>
    /// Configuration information for the Stamp.
    /// </summary>
    public class StampConfigurationOptions
    {
        /// <summary>
        /// Gets or sets the Name of the Stamp.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets a List of Key-Value Pairs of all configuration settings for the object needed for adding to metrics and logs as dimensions.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Values
        {
            get
            {
                var kvps = new List<KeyValuePair<string, object>>
                {
                    new("StampName", Name),
                };
                return kvps;
            }
        }
    }
}

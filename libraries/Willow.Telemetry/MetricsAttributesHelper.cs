namespace Willow.Telemetry
{
    using Microsoft.Extensions.Configuration;
    using Willow.AppContext;

    /// <summary>
    /// Helper class to add Attributes to the Metrics.
    /// </summary>
    public class MetricsAttributesHelper
    {
        private readonly WillowContextOptions? willowContext = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsAttributesHelper"/> class.
        /// </summary>
        /// <param name="configuration">Configuration of the WillowContext.</param>
        public MetricsAttributesHelper(IConfiguration configuration)
        {
            if (configuration != null)
            {
                willowContext = configuration.GetSection("WillowContext").Get<WillowContextOptions>();
            }
        }

        /// <summary>
        /// Get the values from the WillowContext and the passed in key value pairs.
        /// </summary>
        /// <param name="keyValuePair">Any existing key value pairs that need to be added to the metrics in addition to the ones from the Willow context.</param>
        /// <returns>The merged collection of key value pairs.</returns>
        public ReadOnlySpan<KeyValuePair<string, object?>> GetValues(params KeyValuePair<string, object?>[] keyValuePair)
        {
            var list = keyValuePair.ToList();

            if (willowContext != null)
            {
                list.AddRange(willowContext.Values.Where(k => !string.IsNullOrEmpty(k.Value.ToString())));
            }

            return list.ToArray();
        }
    }
}

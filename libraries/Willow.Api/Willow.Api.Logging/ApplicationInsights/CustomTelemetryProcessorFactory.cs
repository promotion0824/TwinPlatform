namespace Willow.Api.Logging.ApplicationInsights
{
    using Microsoft.ApplicationInsights.AspNetCore;
    using Microsoft.ApplicationInsights.Extensibility;
    using Willow.Api.Logging.ApplicationInsights.TelemetryProcessors;

    /// <summary>
    /// The custom telemetry processor factory.
    /// </summary>
    public class CustomTelemetryProcessorFactory : ITelemetryProcessorFactory
    {
        private readonly IgnoreOptions? ignoreOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTelemetryProcessorFactory"/> class.
        /// </summary>
        /// <param name="ignoreOptions">Allows setting of requests and dependencies to ingore.</param>
        public CustomTelemetryProcessorFactory(IgnoreOptions? ignoreOptions)
        {
            this.ignoreOptions = ignoreOptions;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IgnoreTelemetryProcessor"/> class.
        /// </summary>
        /// <param name="nextProcessor">Pointer to the next telemetry processor to add to the pipeline.</param>
        /// <returns>A new instance of the IgnoreTelemetryProcessor.</returns>
        public ITelemetryProcessor Create(ITelemetryProcessor nextProcessor)
        {
            return new IgnoreTelemetryProcessor(nextProcessor, ignoreOptions);
        }
    }
}

namespace Willow.Api.Logging.ApplicationInsights.TelemetryProcessors
{
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// The ignore telemetry processor.
    /// </summary>
    public sealed class IgnoreTelemetryProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor next;
        private readonly IgnoreOptions? ignoreOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreTelemetryProcessor"/> class.
        /// </summary>
        /// <param name="next">The next telemetry processor in the stack.</param>
        /// <param name="ignoreOptions">Settings for what requests or dependencies to ignore in telemetry processing.</param>
        public IgnoreTelemetryProcessor(ITelemetryProcessor next, IgnoreOptions? ignoreOptions)
        {
            this.next = next;
            this.ignoreOptions = ignoreOptions;
        }

        /// <summary>
        /// Processes the telemetry item.
        /// </summary>
        /// <param name="item">The telemetry item to add meta data to or filter.</param>
        public void Process(ITelemetry item)
        {
            if (ignoreOptions is not null)
            {
                if (item is RequestTelemetry requestTelemetry)
                {
                    if (requestTelemetry.Url is not null &&
                        ignoreOptions.Requests?.Paths?.Any(i => requestTelemetry.Url.AbsolutePath.StartsWith(i, StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        return;
                    }

                    if (ignoreOptions.Requests?.Names?.Any(i => requestTelemetry.Name.StartsWith(i, StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        return;
                    }
                }

                if (item is DependencyTelemetry dependencyTelemetry)
                {
                    if (dependencyTelemetry.Name is not null &&
                        ignoreOptions.Dependencies?.Names?.Any(i => dependencyTelemetry.Name.StartsWith(i, StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        return;
                    }
                }
            }

            next.Process(item);
        }
    }
}

namespace Willow.Telemetry;

using OpenTelemetry;
using OpenTelemetry.Logs;
using Willow.AppContext;

/// <summary>
/// Custom LogRecord Enrich Processor for adding <see cref="WillowContextOptions"/> values in to Custom Dimensions.
/// </summary>
public class WillowLogRecordEnrichProcessor : BaseProcessor<LogRecord>
{
    private readonly List<KeyValuePair<string, object>> contextValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="WillowLogRecordEnrichProcessor"/> class.
    /// </summary>
    /// <param name="context">Instance of <see cref="WillowContextOptions"/>.</param>
    public WillowLogRecordEnrichProcessor(WillowContextOptions context)
    {
        contextValues = context.Values.ToList();
    }

    /// <summary>
    /// Overwrite values at the end of telemetry.
    /// </summary>
    /// <param name="data">The log record to which will be added the custom properties.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1010:Opening square brackets should be spaced correctly", Justification = "This is a standard usage in an assignment operation.")]
    public override void OnEnd(LogRecord data)
    {
        var attributes = data.Attributes?.ToList() ?? new List<KeyValuePair<string, object?>>();
        attributes.AddRange(contextValues!);

        data.Attributes = attributes!;
    }
}

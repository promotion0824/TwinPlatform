namespace Willow.Telemetry;

using System.Diagnostics;
using OpenTelemetry;
using Willow.AppContext;

/// <summary>
/// Custom Activity Enrich Processor for adding <see cref="WillowContextOptions"/> values in to Custom Dimensions.
/// </summary>
public class WillowActivityEnrichProcessor : BaseProcessor<Activity>
{
    private readonly List<KeyValuePair<string, object>> contextValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="WillowActivityEnrichProcessor"/> class.
    /// </summary>
    /// <param name="context">Instance of <see cref="WillowContextOptions"/>.</param>
    public WillowActivityEnrichProcessor(WillowContextOptions context)
    {
        contextValues = context.Values.ToList();
    }

    /// <summary>
    /// Overwrite values at the end of telemetry.
    /// </summary>
    /// <param name="data">The telemetry data to have the custom properties added to.</param>
    public override void OnEnd(Activity data)
    {
        contextValues.ForEach(context => data.SetCustomProperty(context.Key, context.Value));
    }
}

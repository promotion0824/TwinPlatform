namespace Willow.Alert.Resolver.ResolutionHandlers.Abstractions;

/// <summary>
/// Resolution context.
/// </summary>
public interface IResolutionContext
{
    /// <summary>
    /// Responses from the handlers.
    /// </summary>
    public Dictionary<string, IResolutionResponse> Responses { get; }
    /// <summary>
    /// Custom properties.
    /// </summary>
    public Dictionary<string, string> CustomProperties { get; }
    /// <summary>
    /// Metrics.
    /// </summary>
    public Dictionary<string, double> Metrics { get; }
}

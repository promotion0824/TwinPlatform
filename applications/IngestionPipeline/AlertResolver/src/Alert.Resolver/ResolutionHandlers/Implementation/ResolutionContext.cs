using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation;

internal sealed class ResolutionContext : IResolutionContext
{
    public Dictionary<string, IResolutionResponse> Responses { get; } = new();
    public Dictionary<string, string> CustomProperties { get; } = new();
    public Dictionary<string, double> Metrics { get; } = new();
}

using Willow.Alert.Resolver.ResolutionHandlers.Enumerations;

namespace Willow.Alert.Resolver.ResolutionHandlers.Abstractions;

/// <summary>
/// Resolution response.
/// </summary>
public interface IResolutionResponse
{
    /// <summary>
    /// Status of the resolution.
    /// </summary>
    ResolutionStatus Status { get; }
    /// <summary>
    /// Message of the resolution.
    /// </summary>
    string Message { get; }
}

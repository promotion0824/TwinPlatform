using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
using Willow.Alert.Resolver.ResolutionHandlers.Enumerations;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation;

internal sealed class ResolutionResponse : IResolutionResponse
{
    private readonly ResolutionStatus _status;

    public ResolutionResponse(ResolutionStatus status, string message)
    {
        _status = status;
        Message = message;
    }
    public ResolutionStatus Status { get { return _status; } }
    public string Message { get; }
}

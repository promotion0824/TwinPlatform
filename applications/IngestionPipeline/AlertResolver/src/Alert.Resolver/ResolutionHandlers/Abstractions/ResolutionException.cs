namespace Willow.Alert.Resolver.ResolutionHandlers.Abstractions;

internal class ResolutionException : Exception
{
    public ResolutionException(string resolutionHandlerName, string message) : base($"Operation:{resolutionHandlerName}-{message}")
    {

    }
}

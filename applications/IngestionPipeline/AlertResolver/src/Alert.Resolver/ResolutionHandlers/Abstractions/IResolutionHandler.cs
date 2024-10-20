namespace Willow.Alert.Resolver.ResolutionHandlers.Abstractions;

internal interface IResolutionHandler<TRequest>
{
    Task<bool> RunAsync(TRequest request, IResolutionContext context, CancellationToken cancellationToken = default);
    Task<bool> RunAsChainAsync(TRequest request, IResolutionContext context, CancellationToken cancellationToken = default);
    void SetNext(IResolutionHandler<TRequest> resolverStep);
}

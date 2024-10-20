namespace Willow.Alert.Resolver.ResolutionHandlers.Abstractions;

internal interface IResolutionStepRunner<TRequest>
{
    Task<bool> RunAsync(TRequest request, IResolutionContext context, CancellationToken cancellationToken = default);
}

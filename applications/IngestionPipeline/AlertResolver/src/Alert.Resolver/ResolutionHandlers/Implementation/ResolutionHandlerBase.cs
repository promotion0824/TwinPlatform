using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
using Willow.Alert.Resolver.ResolutionHandlers.Extensions;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation;

internal abstract class ResolutionHandlerBase<TRequest> : IResolutionHandler<TRequest>
{
    public abstract Task<bool> RunAsync(TRequest request, IResolutionContext context, CancellationToken cancellationToken = default);
    public IResolutionHandler<TRequest>? Next { get; set; }
    public void SetNext(IResolutionHandler<TRequest> resolverStep)
    {
        Next = resolverStep;
    }
    public async Task<bool> RunAsChainAsync(TRequest request, IResolutionContext context, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return false;

        var result = await RunAsync(request, context, cancellationToken);
        context.AddResponse(this, result.GetResolutionStatus());
        if (Next != null)
        {
            return await Next.RunAsChainAsync(request, context, cancellationToken);
        }
        return result;
    }
}

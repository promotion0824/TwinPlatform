using Ardalis.GuardClauses;
using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation;
internal class ResolutionStepRunner<TRequest> : IResolutionStepRunner<TRequest>
{

    private readonly ILogger<ResolutionStepRunner<TRequest>> _logger;
    private readonly List<IResolutionHandler<TRequest>> _resolutionHandlers;
    private IResolutionHandler<TRequest>? _firstHandler;
    const string firstHandlerNullMessage = "First handler is null. Please call Build() method first.";

    public ResolutionStepRunner(ILogger<ResolutionStepRunner<TRequest>> logger, IServiceProvider services)
    {
        _logger = logger;
        _resolutionHandlers = services.GetServices<IResolutionHandler<TRequest>>().ToList();
        Build();
    }


    private void Build()
    {
        //TODO: check if chain is already built
        Guard.Against.Zero(_resolutionHandlers.Count);
        _firstHandler = _resolutionHandlers.First();
        for (int i = 1; i < _resolutionHandlers.Count; i++)
        {
            var firstHandler = _resolutionHandlers[i - 1];
            var secondHandler = _resolutionHandlers[i];
            firstHandler.SetNext(secondHandler);
        }
    }

    public async Task<bool> RunAsync(TRequest request, IResolutionContext context, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(_firstHandler, message: firstHandlerNullMessage);
        return await _firstHandler.RunAsChainAsync(request, context, cancellationToken);
    }
}





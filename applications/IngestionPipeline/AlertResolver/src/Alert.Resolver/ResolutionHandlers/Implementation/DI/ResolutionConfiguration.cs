using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation.DI;
internal class ResolutionConfiguration<TRequest>
{
    private readonly IServiceCollection _services;

    public ResolutionConfiguration(IServiceCollection services)
    {
        _services = services;
    }
    public ResolutionConfiguration<TRequest> AddHandler<THandler>() where THandler : IResolutionHandler<TRequest>
    {
        _services.AddTransient(typeof(IResolutionHandler<TRequest>), typeof(THandler));
        return this;
    }
}

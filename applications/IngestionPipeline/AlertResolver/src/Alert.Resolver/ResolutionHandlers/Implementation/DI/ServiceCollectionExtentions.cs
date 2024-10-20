using System.Reflection;
using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation.DI;

internal static class ServiceCollectionExtentions
{
    public static ResolutionConfiguration<TRequest> ConfigureResolutions<TRequest>(this IServiceCollection services)
    {
        // RegisterHandlers<TRequest>(services, typeof(IResolutionHandler<TRequest>).Assembly);
        services.AddTransient<IResolutionStepRunner<TRequest>, ResolutionStepRunner<TRequest>>();
        return new ResolutionConfiguration<TRequest>(services);
    }

    public static void RegisterHandlers<TRequest>(this IServiceCollection services,
                                                    params Assembly[] assemblies)
    {
        var handlerTypes = assemblies.SelectMany(a => a.DefinedTypes)
                                     .Where(t => t.ImplementedInterfaces.Contains(typeof(IResolutionHandler<TRequest>)) && !t.IsAbstract);

        foreach (var typeInfo in handlerTypes)
        {
            services.AddTransient(typeof(IResolutionHandler<TRequest>),typeInfo);
        }
    }
}

namespace Willow.ServiceBus;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Willow.ServiceBus.Options;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to add the Service Bus.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Configures IServiceBusClientFactory that manages service bus clients,
    /// senders and processors and their lifetime.
    /// </summary>
    /// <param name="services">to add the services to.</param>
    /// <param name="section">The section where servicebus setting are set.</param>
    /// <returns>The updated services collection.</returns>
    public static IServiceCollection AddServiceBus(this IServiceCollection services, IConfigurationSection section)
    {
        services
            .AddOptions<ServiceBusOptions>()
            .Bind(section);

        services.AddSingleton<IServiceBusClientFactory, ServiceBusClientFactory>();
        services.AddTransient<IMessageSender, MessageSender>();
        services.AddTransient<IMessageConsumer, MessageConsumer>();

        return services;
    }
}

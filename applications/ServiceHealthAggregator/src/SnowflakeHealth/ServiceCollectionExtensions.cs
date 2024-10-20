namespace Microsoft.Extensions.DependencyInjection;

using global::Azure.Core;
using global::Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Willow.Email.SendGrid;
using Willow.ServiceHealthAggregator.Snowflake;
using Willow.ServiceHealthAggregator.Snowflake.Options;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Snowflake listener to the service collection.
    /// </summary>
    /// <param name="services">The services collection that this method extends.</param>
    /// <param name="configure">Configures the Snowflake listener.</param>
    /// <returns>The service collection so that calls can be chained.</returns>
    public static IServiceCollection AddSnowflakeListener(this IServiceCollection services, Action<SnowflakeOptions> configure)
    {
        services.AddSingleton(services =>
        {
            var options = services.GetRequiredService<IOptions<SnowflakeOptions>>().Value;
            var cred = services.GetRequiredService<TokenCredential>();
            return new ServiceBusClient(options.ServiceBus.FullyQualifiedNamespace, cred);
        });

        services.Configure(configure);
        services.AddSendGrid();
        services.AddSingleton<IMessageForwarder, MultiMessageForwarder>();
        services.AddHostedService<Listener>();
        return services;
    }
}

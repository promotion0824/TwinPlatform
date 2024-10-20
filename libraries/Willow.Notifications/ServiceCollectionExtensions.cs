namespace Willow.Notifications;

using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Services;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to add the Notifications Service.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Notifications Service to the service collection.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="options">A function that provides configuration options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddNotificationsService(this IServiceCollection services, Action<NotificationsServiceOptions> options)
    {
        services.Configure(options);
        var optionsValue = services.BuildServiceProvider()
            .GetRequiredService<IOptions<NotificationsServiceOptions>>().Value;
        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClient(optionsValue.ServiceBusConnectionString);
        });
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}

/// <summary>
/// Configuration options for the Notifications Service.
/// </summary>
public class NotificationsServiceOptions
{
    /// <summary>
    /// Gets or sets connection string to the Azure Service Bus.
    /// </summary>
    public required string ServiceBusConnectionString { get; set; }

    /// <summary>
    /// Gets or sets name of the queue to send messages to.
    /// </summary>
    public required string QueueOrTopicName { get; set; }
}

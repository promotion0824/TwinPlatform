namespace Microsoft.Extensions.DependencyInjection;

using global::Azure.Identity;
using global::Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Willow.LiveData.Pipeline;
using Willow.LiveData.Pipeline.Configuration;
using Willow.LiveData.Pipeline.EventHub;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the default Event Hub sender to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance that this method extends.</param>
    /// <param name="options">Configures the Event Hub listener.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddEventHubSender(this IServiceCollection services, Action<EventHubConfig> options)
    {
        services.Configure(options);

        services.TryAddSingleton<EventHubClientFactory>();
        services.AddSingleton<ISender, EventHubSender>();
        return services;
    }

    /// <summary>
    /// Adds the Event Hub sender for specified telemetry type to the service collection.
    /// </summary>
    /// <typeparam name="TTelemetry">The type of telemetry that will be sent.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance that this method extends.</param>
    /// <param name="options">Configures the Event Hub listener.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddEventHubSender<TTelemetry>(this IServiceCollection services, Action<EventHubConfig> options)
    {
        services.Configure(options);

        services.TryAddSingleton<EventHubClientFactory>();
        services.AddSingleton<ISender<TTelemetry>, EventHubSender<TTelemetry>>();
        return services;
    }

    /// <summary>
    /// Adds a hosted service that will listen for telemetry and send it to the specified processor.
    /// </summary>
    /// <typeparam name="TTelemetryProcessor">The type of processor that will handle the message.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance that this method extends.</param>
    /// <param name="options">Configures the Event Hub listener.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddEventHubListener<TTelemetryProcessor>(this IServiceCollection services, Action<EventHubConfig> options)
        where TTelemetryProcessor : class, ITelemetryProcessor
    {
        services.AddEventHubListener<Telemetry, TTelemetryProcessor>(options);
        services.AddSingleton<ITelemetryProcessor, TTelemetryProcessor>();
        services.AddTelemetryProcessorHealthCheck();

        return services;
    }

    /// <summary>
    /// Adds a hosted service that will listen for telemetry of the specified type, and send it to the specified processor.
    /// </summary>
    /// <typeparam name="TTelemetry">The type of telemetry that will be received.</typeparam>
    /// <typeparam name="TTelemetryProcessor">The type of processor that will handle the message.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance that this method extends.</param>
    /// <param name="options">Configures the Event Hub listener.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddEventHubListener<TTelemetry, TTelemetryProcessor>(this IServiceCollection services, Action<EventHubConfig> options)
        where TTelemetryProcessor : class, ITelemetryProcessor<TTelemetry>
    {
        services.Configure(options);

        services.TryAddSingleton<EventHubClientFactory>();
        services.AddSingleton<ITelemetryProcessor<TTelemetry>, TTelemetryProcessor>();
        services.AddHostedService<TelemetryListener<TTelemetry>>();

        return services;
    }

    /// <summary>
    /// Adds a hosted service that will listen for telemetry and send it in batches to the specified processor.
    /// </summary>
    /// <typeparam name="TTelemetryProcessor">The type of processor that will handle the message.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance that this method extends.</param>
    /// <param name="options">Configures the Event Hub listener.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddBatchEventHubListener<TTelemetryProcessor>(this IServiceCollection services, Action<EventHubConfig> options)
        where TTelemetryProcessor : class, ITelemetryProcessor
    {
        services.AddBatchEventHubListener<Telemetry, TTelemetryProcessor>(options);
        services.AddSingleton<ITelemetryProcessor, TTelemetryProcessor>();

        return services;
    }

    /// <summary>
    /// Adds a hosted service that will listen for telemetry of the specified type, and send it in batches to the specified processor.
    /// </summary>
    /// <typeparam name="TTelemetry">The type of telemetry that will be received.</typeparam>
    /// <typeparam name="TTelemetryProcessor">The type of processor that will handle the message.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance that this method extends.</param>
    /// <param name="options">Configures the Event Hub listener.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddBatchEventHubListener<TTelemetry, TTelemetryProcessor>(this IServiceCollection services, Action<EventHubConfig> options)
        where TTelemetryProcessor : class, ITelemetryProcessor<TTelemetry>
    {
        services.Configure(options);

        services.AddSingleton<ITelemetryProcessor<TTelemetry>, TTelemetryProcessor>();
        services.AddSingleton<IBatchProcessor, EventHubBatchProcessor<TTelemetry>>();
        services.AddHostedService<BatchTelemetryListener<TTelemetry>>();

        services.AddTelemetryProcessorHealthCheck();

        services.AddSingleton<global::Azure.Core.TokenCredential, DefaultAzureCredential>();

        services.AddSingleton(services =>
        {
            var config = services.GetRequiredService<IOptions<EventHubConfig>>().Value;

            config.ThrowIfSourceNull();

            var containerClient = new BlobContainerClient(new Uri(config.Source!.StorageAccountUri, config.Source.StorageContainerName), new DefaultAzureCredential());
            containerClient.CreateIfNotExists();

            return containerClient;
        });

        return services;
    }

    /// <summary>
    /// Adds a hosted service that will listen for telemetry of the specified type, and send it in batches to the specified processor.
    /// </summary>
    /// <typeparam name="TTelemetry">The type of telemetry that will be received.</typeparam>
    /// <typeparam name="TTelemetryProcessor">The type of processor that will handle the message.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance that this method extends.</param>
    /// <param name="options">Configures the Event Hub listener.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddIotHubListener<TTelemetry, TTelemetryProcessor>(this IServiceCollection services, Action<EventHubConfig> options)
        where TTelemetryProcessor : class, ITelemetryProcessor<TTelemetry>
    {
        services.Configure(options);

        services.AddSingleton<ITelemetryProcessor<TTelemetry>, TTelemetryProcessor>();
        services.AddSingleton<IBatchProcessor, IoTHubBatchProcessor<TTelemetry>>();
        services.AddHostedService<BatchTelemetryListener<TTelemetry>>();

        services.AddTelemetryProcessorHealthCheck();

        services.AddSingleton<global::Azure.Core.TokenCredential, DefaultAzureCredential>();

        services.AddSingleton(services =>
        {
            var config = services.GetRequiredService<IOptions<EventHubConfig>>().Value;
            config.ThrowIfSourceNull();
            var containerClient = new BlobContainerClient(new Uri(config.Source!.StorageAccountUri, config.Source.StorageContainerName), new DefaultAzureCredential());
            containerClient.CreateIfNotExists();

            return containerClient;
        });

        return services;
    }

    /// <summary>
    /// Register processor filters within the specified assembly.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance that this method extends.</param>
    /// <param name="lifetime">Lifetime of the filters.</param>
    /// <param name="includeInternal">Default to true, set false if only public filters should be registered.</param>
    /// <typeparam name="T">Type of IProcessFilter where filters are present.</typeparam>
    /// <returns>The <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddProcessorFiltersFromAssembly<T>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient, bool includeInternal = true)
        where T : IProcessorFilter
    {
        var assembly = typeof(T).Assembly;
        var types = includeInternal ? assembly.GetTypes() : assembly.GetExportedTypes();
        var filters = types
            .Where(type => type is { IsAbstract: false, IsClass: true } && typeof(IProcessorFilter).IsAssignableFrom(type));

        foreach (var filter in filters)
        {
            services.Add(new ServiceDescriptor(typeof(IProcessorFilter), filter, lifetime));
        }

        return services;
    }

    /// <summary>
    /// Adds a <see cref="HealthCheckTelemetryProcessor"/> health check.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance that this method extends.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddTelemetryProcessorHealthCheck(this IServiceCollection services)
    {
        services.AddSingleton<HealthCheckTelemetryProcessor>()
                .AddHealthChecks().AddCheck<HealthCheckTelemetryProcessor>("TelemetryProcessor");

        return services;
    }

    /// <summary>
    /// Adds a test listener that generates fake telemetry every 5 seconds.
    /// </summary>
    /// <typeparam name="TTelemetryProcessor">The type of processor that will handle the message.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance that this method extends.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddTestListener<TTelemetryProcessor>(this IServiceCollection services)
    where TTelemetryProcessor : class, ITelemetryProcessor
    {
        services.AddSingleton<ITelemetryProcessor, TTelemetryProcessor>();
        services.AddHostedService<TestListener>();

        return services;
    }

    /// <summary>
    /// Adds a test listener that generates fake telemetry every 5 seconds.
    /// </summary>
    /// <typeparam name="TTelemetry">The type of telemetry that will be received.</typeparam>
    /// <typeparam name="TTelemetryProcessor">The type of processor that will handle the message.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance that this method extends.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddTestListener<TTelemetry, TTelemetryProcessor>(this IServiceCollection services)
        where TTelemetryProcessor : class, ITelemetryProcessor<TTelemetry>
    {
        services.AddSingleton<ITelemetryProcessor<TTelemetry>, TTelemetryProcessor>();
        services.AddHostedService<TestListener>();

        return services;
    }
}

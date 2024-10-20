// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" Company="Willow">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.TopologyIngestion.Extensions
{
    using System.Net.Http.Headers;
    using System.Reflection;
    using Azure.Core;
    using Azure.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Willow.Api.Authentication;
    using Willow.AzureDigitalTwins.SDK.Client;
    using Willow.TopologyIngestion.AzureDigitalTwins;
    using Willow.TopologyIngestion.Interfaces;
    using Willow.TopologyIngestion.Mapped;

    /// <summary>
    /// Static extension method class for adding an ingestion manager onto a <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an ingestion manager to an IServiceCollection for DI-based deployment.
        /// </summary>
        /// <typeparam name="TOptions">Ingestion manager options type.</typeparam>
        /// <param name="services">Collection of service descriptors to which Ingestion Manager will be added.</param>
        /// <param name="options">Ingestion manager options.</param>
        /// <returns>Collection of service descriptors to which Ingestion Manager has been added.</returns>
        public static IServiceCollection AddIngestionManager<TOptions>(this IServiceCollection services, Action<TOptions> options)
            where TOptions : MappedIngestionManagerOptions
        {
            services.AddOptions<TOptions>()
                    .Configure(options)
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

            services.AddHttpClient("Willow.TopologyIngestion");
            services.AddSingleton<IGraphNamingManager, DefaultGraphNamingManager>();
            services.AddTransient<IOutputGraphManager, AdtApiGraphManager<TOptions>>();

            var ingestionOptions = new MappedIngestionManagerOptions();
            options?.Invoke((TOptions)ingestionOptions);

            services.AddHttpClient<ITwinsClient, TwinsClient>(ConfigureHttpClient(ingestionOptions))
                    .AddHttpMessageHandler<AuthenticationDelegatingHandler>();
            services.AddHttpClient<IRelationshipsClient, RelationshipsClient>(ConfigureHttpClient(ingestionOptions))
                    .AddHttpMessageHandler<AuthenticationDelegatingHandler>();
            services.AddHttpClient<IModelsClient, ModelsClient>(ConfigureHttpClient(ingestionOptions))
                    .AddHttpMessageHandler<AuthenticationDelegatingHandler>();
            services.AddHttpClient<IMappingClient, MappingClient>(ConfigureHttpClient(ingestionOptions))
                    .AddHttpMessageHandler<AuthenticationDelegatingHandler>();
            services.AddHttpClient<IJobsClient, JobsClient>(ConfigureHttpClient(ingestionOptions))
                    .AddHttpMessageHandler<AuthenticationDelegatingHandler>();
            return services;
        }

        /// <summary>
        /// Adds a Mapped ingestion manager to an IServiceCollection, for DI-based deployment.
        /// </summary>
        /// <param name="services">Collection of service descriptors to which Mapped Ingestion Manager will be added.</param>
        /// <param name="options">Mapped ingestion manager options.</param>
        /// <returns>Collection of service descriptors to which Mapped Ingestion Manager has been added.</returns>
        public static IServiceCollection AddMappedIngestionManager(this IServiceCollection services, Action<MappedIngestionManagerOptions> options)
        {
            services.AddOptions<MappedIngestionManagerOptions>()
                    .Configure(options)
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

            services.AddTransient<IInputGraphManager, MappedGeneratedGraphManager>();
            services.AddTransient<IGraphIngestionProcessor, MappedGraphIngestionProcessor<MappedIngestionManagerOptions>>();

            services.AddIngestionManager(options);

            return services;
        }

        private static Action<IServiceProvider, HttpClient> ConfigureHttpClient(IngestionManagerOptions option) => (serviceProvider, httpClient) =>
        {
            httpClient.BaseAddress = new Uri(option.AdtApiEndpoint);
        };
    }
}

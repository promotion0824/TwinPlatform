namespace Willow.DataAccess.CosmosDb;

using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Willow.DataAccess.CosmosDb.Options;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add CosmosDb to the service collection.
    /// </summary>
    /// <param name="services">The list of services for the applciation.</param>
    /// <param name="cosmosDbConfigSection">The configuration section for the CosmosDB from app settings.</param>
    /// <param name="propertyNamingPolicy">The property naming policy for the CosmosSerializer Options.</param>
    /// <returns>An updated services collection.</returns>
    public static IServiceCollection AddCosmosDb(
        this IServiceCollection services,
        IConfigurationSection cosmosDbConfigSection,
        CosmosPropertyNamingPolicy propertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase)
    {
        var options = new CosmosDbOptions();
        cosmosDbConfigSection.Bind(options);

        services.AddSingleton(_ =>
        {
            var cosmosOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = propertyNamingPolicy,
                },
            };
            return options.UseManagedIdentity
                ? new CosmosClient(new Uri(options.EndpointUrl).AbsoluteUri, new DefaultAzureCredential(), cosmosOptions)
                : new CosmosClient(options.ConnectionString, cosmosOptions);
        });

        return services;
    }
}

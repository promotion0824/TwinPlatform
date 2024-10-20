using System;
using System.Threading;
using DirectoryCore.Services;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Willow.Security.KeyVault;
using Willow.Security.KeyVault.Options;

namespace DirectoryCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRetryPipelines(this IServiceCollection services)
    {
        // For transient faults that may self-correct after a short delay.
        services.AddResiliencePipeline(
            ResiliencePipelineName.Retry,
            pipelineBuilder =>
            {
                var retryOptions = new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential
                };
                pipelineBuilder.AddRetry(retryOptions);
            }
        );

        services.AddScoped<IResiliencePipelineService, ResiliencePipelineService>();

        return services;
    }

    /// <summary>
    /// Add SecretManager to the service collection.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="azureKeyVaultSection">Key Vault Section</param>
    /// <returns><see cref="IServiceCollection"/></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddSecretManager(
        this IServiceCollection services,
        IConfigurationSection azureKeyVaultSection
    )
    {
        var keyVaultOptions = azureKeyVaultSection.Get<KeyVaultOptions>();
        var vaultName = keyVaultOptions.KeyVaultName;

        if (string.IsNullOrEmpty(vaultName))
        {
            throw new Exception("Azure:KeyVault:KeyVaultName config setting is missing");
        }

        // This semaphore is used by SecretManager
        (object key, Semaphore semaphore) = SecretManager.GetKeyedSingletonDependencies();
        services.AddKeyedSingleton(key, semaphore);

        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddSecretClient(new Uri($"https://{vaultName}.vault.azure.net/"));
        });

        services.AddSingleton<ISecretManager, SecretManager>();

        return services;
    }
}

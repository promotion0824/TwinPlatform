using DigitalTwinCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace DigitalTwinCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRetryPipelines(this IServiceCollection services)
    {
        // For transient faults that may self-correct after a short delay.
        services.AddResiliencePipeline(ResiliencePipelineName.Retry, pipelineBuilder =>
        {
            var retryOptions = new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential
            };
            pipelineBuilder.AddRetry(retryOptions);
        });

        services.AddScoped<IResiliencePipelineService, ResiliencePipelineService>();

        return services;
    }
}

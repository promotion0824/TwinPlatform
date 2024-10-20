namespace Willow.CommandAndControlAPI.SDK.Extensions;

using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Willow.Api.Authentication;
using Willow.CommandAndControlAPI.SDK.Client;
using Willow.CommandAndControlAPI.SDK.Option;

/// <summary>
/// Dependency Injection extension methods for CommandAndControlAPI SDK.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add CommandAndControlAPI HTTP client.
    /// </summary>
    /// <param name="services">services.</param>
    /// <param name="clientOption">clientOption.</param>
    /// <returns>The services collection so that calls can be chained.</returns>
    public static IServiceCollection AddCommandAndControlAPIHttpClient(this IServiceCollection services,
            CommandAndControlClientOption clientOption)
    {
        services.AddHttpClient<ICommandAndControlClient, CommandAndControlClient>((sp, httpClient) =>
            {
                httpClient.BaseAddress = new Uri(clientOption.BaseAddress);
                var tokenService = sp.GetRequiredService<IClientCredentialTokenService>();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.HeaderBearer, tokenService.GetClientCredentialToken());
                httpClient.DefaultRequestHeaders.Add("Authorization-Scheme", "AzureAd");
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            });

        return services;
    }
}

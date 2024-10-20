using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Willow.Api.Authentication;
using Willow.AzureDigitalTwins.SDK.Option;
using System.Net.Http.Headers;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Api.Common.Runtime;

namespace Willow.AzureDigitalTwins.SDK.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public const string AdtApiClientName = "AdtApiClient";

        private static Action<IServiceProvider, HttpClient> ConfigureHttpClient(AdtApiClientOption option)
            => (serviceProvider, httpClient) =>
            {
                httpClient.BaseAddress = new Uri(option.BaseAddress);
                var tokenService = serviceProvider.GetRequiredService<IClientCredentialTokenService>();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.HeaderBearer, tokenService.GetClientCredentialToken());

                using var scope = serviceProvider.CreateScope();
                var currentHttpContext = scope.ServiceProvider.GetService<ICurrentHttpContext>();
                if (currentHttpContext != null && !string.IsNullOrWhiteSpace(currentHttpContext.UserEmail))
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Id", currentHttpContext.UserEmail);
                }
                httpClient.Timeout = option.Timeout;
            };

        private static void ConfigureRequiredAuthServices(IServiceCollection services)
        {
            // Configure Authentication Services
            services.AddSingleton<IClientCredentialTokenService, ClientCredentialTokenService>();
            services.AddTransient<ICurrentHttpContext, CurrentHttpContext>();
        }

        private static void ConfigureAllEndpointClients(IServiceCollection services, AdtApiClientOption option)
        {
            // Configure Clients
            services.AddHttpClient<ICacheClient, CacheClient>(ConfigureHttpClient(option));
            services.AddHttpClient<IDocumentsClient, DocumentsClient>(ConfigureHttpClient(option));

            services.AddHttpClient<IGraphClient, GraphClient>(ConfigureHttpClient(option));
            services.AddHttpClient<IImportClient, ImportClient>(ConfigureHttpClient(option));
            services.AddHttpClient<IModelsClient, ModelsClient>(ConfigureHttpClient(option));

            services.AddHttpClient<IQueryClient, QueryClient>(ConfigureHttpClient(option));
            services.AddHttpClient<IRelationshipsClient, RelationshipsClient>(ConfigureHttpClient(option));
            services.AddHttpClient<ITwinsClient, TwinsClient>(ConfigureHttpClient(option));

            services.AddHttpClient<IDQRuleClient, DQRuleClient>(ConfigureHttpClient(option));
            services.AddHttpClient<IDQValidationClient, DQValidationClient>(ConfigureHttpClient(option));

            services.AddHttpClient<ISearchClient, SearchClient>(ConfigureHttpClient(option));

            services.AddHttpClient<IMappingClient, MappingClient>(ConfigureHttpClient(option));
            services.AddHttpClient<IEnvClient, EnvClient>(ConfigureHttpClient(option));
            services.AddHttpClient<ITimeSeriesClient, TimeSeriesClient>(ConfigureHttpClient(option));
            services.AddHttpClient<IJobsClient, JobsClient>(ConfigureHttpClient(option));
        }


        public static void AddAdtApiHttpClient(this IServiceCollection services,
                            string baseUrl, string bearerToken, string? UserId = null)
        {
            services.AddHttpClient(AdtApiClientName, (serviceProvider, httpClient) =>
            {
                httpClient.BaseAddress = new Uri(baseUrl);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.HeaderBearer, bearerToken);
                if (!string.IsNullOrWhiteSpace(UserId))
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Id", UserId);
            });
        }

        public static void AddAdtApiHttpClient(this IServiceCollection services, IConfigurationSection adtApiClientConfigSection)
        {
            ConfigureRequiredAuthServices(services);
            services.AddHttpClient(AdtApiClientName, ConfigureHttpClient(adtApiClientConfigSection.Get<AdtApiClientOption>()!));
        }

        public static void AddAdtApiClients(this IServiceCollection services, IConfigurationSection adtApiClientConfigSection)
        {
            ConfigureRequiredAuthServices(services);
            ConfigureAllEndpointClients(services, adtApiClientConfigSection.Get<AdtApiClientOption>()!);
        }

        public static void AddAdtApiClients(this IServiceCollection services, AdtApiClientOption option)
        {
            ConfigureRequiredAuthServices(services);
            ConfigureAllEndpointClients(services, option);
        }
    }
}

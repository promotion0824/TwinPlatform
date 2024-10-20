using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scheduler.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Scheduler;
public class Program
{
    public static IConfigurationRoot Configuration { get; set; }

    public static async Task Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureAppConfiguration((context, config) =>
            {
                Configuration = config
                                #if DEBUG
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
                                #endif
                                .AddEnvironmentVariables()
                                .Build();


                config.AddConfiguration(Configuration);
            })
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddScoped<IWorkflowApi, WorkflowApi>();
                services.AddMemoryCache();
                services.AddHttpClient();
                services.AddHttpClient(ApiServiceNames.WorkflowCore, (sv, client) =>
               {
                   var config = sv.GetRequiredService<IConfiguration>();
                   client.BaseAddress = new Uri(config.GetValue<string>("WorkflowCoreBaseAddress"));
                   string token = FetchMachineToMachineToken(sv, config).Result;
                   if (!string.IsNullOrWhiteSpace(token))
                   {
                       client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                   }
               }
              );

            }).Build();

        await host.RunAsync();
    }

    private static async Task<string> FetchMachineToMachineToken(IServiceProvider services, IConfiguration configuration)
    {
        var domain = configuration["M2MAuthDomain"];
        var clientId = configuration["M2MAuthClientId"];
        var clientSecret = configuration["M2MAuthClientSecret"];
        var audience = configuration["M2MAuthAudience"];

        var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        var cache = services.GetRequiredService<IMemoryCache>();
        var logger = services.GetRequiredService<ILogger<ServiceCollection>>();
        var token = await cache.GetOrCreateWithLockAsync(
            "MachineToMachineToken",
            async (cacheEntry) =>
            {
                using (var client = httpClientFactory.CreateClient())
                {
                    client.BaseAddress = new Uri("https://" + domain);
                    var response = await client.PostAsJsonAsync("oauth/token", new
                    {
                        client_id = clientId,
                        client_secret = clientSecret,
                        audience = audience,
                        grant_type = "client_credentials"
                    });

                    if (!response.IsSuccessStatusCode)
                    {
                        var responseBody = string.Empty;
                        try
                        {
                            responseBody = await response.Content.ReadAsStringAsync();
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to get access token. Http status code: {statusCode} Failed to get ResponseBody. {exceptionMessage}", response.StatusCode, ex.Message);
                            throw;
                        }
                        throw new HttpRequestException($"Failed to get access token. Http status code: {response.StatusCode} ResponseBody: {responseBody}");
                    }
                    var tokenResponse = await response.Content.ReadAsAsync<TokenResponse>();
                    logger.LogInformation("Succeeded to get access token. ExpiresIn: {expiresIn}", tokenResponse.ExpiresIn);

                     cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 100);
                    return tokenResponse.AccessToken;
                }
            }
        );
        return token;
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }
}

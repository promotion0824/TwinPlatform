using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Willow.Infrastructure;
using Willow.Infrastructure.Exceptions;
using Willow.Infrastructure.MultiRegion;
using Willow.Infrastructure.Services;
using Willow.Infrastructure.Swagger;

    
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                return settings;
            });

            services.AddMemoryCache();
            services.AddHttpContextAccessor();

            services.AddMvc(options => {
                options.Filters.Add<RestExceptionFilter>();
            })
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                    opt.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                });
                
            var needMultiRegion = false;
            var multiRegionSettings = new MultiRegionSettings(configuration);
            var httpClientConfiguration = configuration.GetSection("HttpClientFactory");
            foreach (var clientConfiguration in httpClientConfiguration.GetChildren())
            {
                var apiName = clientConfiguration.Key;
                var baseAddress = clientConfiguration["BaseAddress"];
                if (MultiRegionHelper.IsMultRegionUrl(baseAddress))
                {
                    AddRegionalHttpClients(services, apiName, clientConfiguration, multiRegionSettings);
                    needMultiRegion = true;
                }
                else
                {
                    AddHttpClient(services, apiName, clientConfiguration);
                }
            }
            if (needMultiRegion)
            {
                services.AddSingleton<IMultiRegionSettings, MultiRegionSettings>();
                services.AddSingleton<IMachineToMachineTokenAgent, MachineToMachineTokenAgent>();
            }

            services.AddCors();

            if (configuration.GetValue<bool>("EnableSwagger"))
            {
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = Assembly.GetEntryAssembly().GetName().Name, Version = "1" });
                    options.EnableAnnotations();
                    options.CustomSchemaIds(x => x.Name);

                    if (configuration.GetValue<string>("Auth0:ClientId") != null)
                    {
                        options.OperationFilter<SecurityRequirementsOperationFilter>();
                        var authServerUrl = $"https://{configuration.GetValue<string>("Auth0:Domain")}";
                        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.OAuth2,
                            Flows = new OpenApiOAuthFlows
                                {
                                    Implicit = new OpenApiOAuthFlow
                                    {
                                        AuthorizationUrl = new Uri(authServerUrl + "/authorize"),
                                        TokenUrl = new Uri(authServerUrl + "/oauth/token"),
                                    }
                                }
                         });
                    }
                });
            }

            services.AddSingleton<IDateTimeService, DateTimeService>();
        }

        private static void AddHttpClient(IServiceCollection services, string apiName, IConfigurationSection clientConfiguration)
        {
            services.AddHttpClient(apiName, (sv, client) =>
            {
                client.BaseAddress = new Uri(clientConfiguration["BaseAddress"]);
                var authenticationSection = clientConfiguration.GetSection("Authentication");
                if (!authenticationSection.GetChildren().Any())
                {
                    return;
                }

                SetupHttpClient(client, apiName, authenticationSection, sv);
            });
        }

        private static void SetupHttpClient(HttpClient client, string apiName, IConfigurationSection authenticationSection, IServiceProvider serviceProvider)
        {
            var scheme = authenticationSection["Scheme"];
            if (scheme == "TokenFromCookie")
            {
                string token = GetTokenFromAuthenticationHeaderOrCookie(serviceProvider);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            else if (scheme == "PassDownAuthorization")
            {
                var authValue = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request?.Headers["Authorization"];
                if (authValue.HasValue && authValue.Value.Count > 0)
                {
                    client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authValue.Value[0]);
                }
            }
            else if (scheme == "MachineToMachine")
            {
                string token = FetchMachineToMachineToken(apiName, serviceProvider, authenticationSection).Result;
                if (!string.IsNullOrWhiteSpace(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            else
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    scheme,
                    authenticationSection["Parameter"]);
            }
        }

        private static string GetTokenFromAuthenticationHeaderOrCookie(IServiceProvider serviceProvider)
        {
            var authValue = serviceProvider.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.Request?.Headers["Authorization"];
            if (authValue.HasValue && authValue.Value.Count > 0)
            {
                return authValue.Value[0].Substring("Bearer ".Length);
            }
            else
            {
                return serviceProvider.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.User?.FindFirst(ClaimTypes.Authentication)?.Value;
            }
        }

        private static async Task<string> FetchMachineToMachineToken(string apiName, IServiceProvider services, IConfigurationSection authenticationSection)
        {
            var domain = authenticationSection["Domain"];
            var clientId = authenticationSection["ClientId"];
            var clientSecret = authenticationSection["ClientSecret"];
            var audience = authenticationSection["Audience"];

            var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
            var cache = services.GetRequiredService<IMemoryCache>();
            var logger = services.GetRequiredService<ILogger<ServiceCollection>>();
            var token = await cache.GetOrCreateAsync(
                "MachineToMachineTokens_" + apiName,
                async (cacheEntry) =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    using (var client = httpClientFactory.CreateClient())
                    {
                        client.BaseAddress = new Uri("https://" + domain);
                        var response = await client.PostAsJsonAsync("oauth/token", new {
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
                            catch(Exception ex)
                            {
                                logger.LogError(ex, "Failed to get access token. Http status code: {statusCode} Failed to get ResponseBody. {exceptionMessage}", response.StatusCode, ex.Message);
                                throw;
                            }
                            throw new HttpRequestException($"Failed to get access token. Http status code: {response.StatusCode} ResponseBody: {responseBody}");
                        }
                        var tokenResponse = await response.Content.ReadAsAsync<MachineToMachineTokenAgent.TokenResponse>();
                        return tokenResponse.AccessToken;
                    }
                }
            );
            return token;
        }

        private static void AddRegionalHttpClients(
            IServiceCollection services,
            string apiName,
            IConfigurationSection clientConfiguration,
            MultiRegionSettings multiRegionSettings)
        {
            var authenticationSection = clientConfiguration.GetSection("Authentication");
            if (!authenticationSection.GetChildren().Any())
            {
                throw new InvalidOperationException($"Missing 'Authentication' for HttpClient {apiName}");
            }
            if (authenticationSection["Scheme"] != "MachineToMachinePerRegion")
            {
                throw new InvalidOperationException($"Unknown scheme for HttpClient {apiName}");
            }

            foreach(var region in multiRegionSettings.Regions)
            {
                var regionId = region.Id;
                var regionalApiName = MultiRegionHelper.ServiceName(apiName, regionId);
                services.AddHttpClient(regionalApiName, (sv, client) =>
                {
                    var baseAddress = clientConfiguration["BaseAddress"];
                    client.BaseAddress = new Uri(MultiRegionHelper.MultiRegionUrl(baseAddress, regionId));
                    var tokenAgent = sv.GetRequiredService<IMachineToMachineTokenAgent>();
                    var token = tokenAgent.GetToken(regionId);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                });
            }
        }

        public static void AddApplicationPart(this IServiceCollection services, Assembly assembly)
        {
            var managerService = services.FirstOrDefault(s => s.ServiceType == typeof(ApplicationPartManager));
            if (managerService != null)
            {
                var applicationParts = ((ApplicationPartManager)managerService.ImplementationInstance).ApplicationParts;
                var exists = applicationParts.Any(p => (p is AssemblyPart) && ((AssemblyPart)p).Assembly == assembly);
                if (!exists)
                {
                    var part = new AssemblyPart(assembly);
                    applicationParts.Add(part);
                }
            }
        }

        public static IServiceCollection ReplaceSingleton<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
        {
            var service = services.First(s => s.ServiceType == typeof(TService));
            services.Remove(service);
            return services.AddSingleton<TService, TImplementation>();
        }

        public static IServiceCollection ReplaceSingleton(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            var service = services.First(s => s.ServiceType == serviceType);
            services.Remove(service);
            return services.AddSingleton(serviceType, implementationType);
        }

        public static IServiceCollection ReplaceSingleton(this IServiceCollection services, Type serviceType, object implementationInstance)
        {
            var service = services.First(s => s.ServiceType == serviceType);
            services.Remove(service);
            return services.AddSingleton(serviceType, implementationInstance);
        }

        public static IServiceCollection ReplaceScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            var service = services.First(s => s.ServiceType == typeof(TService));
            services.Remove(service);
            return services.AddScoped(implementationFactory);
        }

        public static IServiceCollection ReplaceScoped<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
        {
            var service = services.First(s => s.ServiceType == typeof(TService));
            services.Remove(service);
            return services.AddScoped<TService, TImplementation>();
        }

    }
}

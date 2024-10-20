using LazyCache;
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
using Willow.Common;
using Willow.Infrastructure;
using Willow.Infrastructure.Exceptions;
using Willow.Infrastructure.MultiRegion;
using Willow.Infrastructure.Swagger;
using WorkflowCore.Services;

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
                settings.Converters.Add(new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() });
                return settings;
            });

            services.AddMemoryCache();
            services.AddHttpContextAccessor();

            var needMultiRegion = false;
            var multiRegionSettings = new MultiRegionSettings(configuration);
            var httpClientConfiguration = configuration.GetSection("HttpClientFactory");
            var serviceKey = configuration["ServiceKeyAuth:ServiceKey1"] ?? "";

            foreach (var clientConfiguration in httpClientConfiguration.GetChildren())
            {
                var apiName = clientConfiguration.Key;
                var baseAddress = clientConfiguration["BaseAddress"];
                if (MultiRegionHelper.IsMultRegionUrl(baseAddress))
                {
                    AddRegionalHttpClients(services, apiName, clientConfiguration, multiRegionSettings, serviceKey);
                    needMultiRegion = true;
                }
                else
                {
                    AddHttpClient(services, apiName, clientConfiguration, serviceKey);
                }
            }
            if (needMultiRegion)
            {
                services.AddSingleton<IMultiRegionSettings, MultiRegionSettings>();
                services.AddSingleton<IMachineToMachineTokenAgent, MachineToMachineTokenAgent>();
            }

            services.AddCors();

            services.AddMvc(options =>
            {
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

        private static void AddHttpClient(
            IServiceCollection services,
            string apiName,
            IConfigurationSection clientConfiguration,
            string serviceKey)
        {
            services.AddHttpClient(apiName, (sv, client) =>
            {
                client.BaseAddress = new Uri(clientConfiguration["BaseAddress"]);
                SetServiceKeyHeader(serviceKey, client.DefaultRequestHeaders, clientConfiguration);
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
            var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
            var cache = services.GetRequiredService<IAppCache>();
            var logger = services.GetRequiredService<ILogger<Auth0Service>>();
            var auth0Service = new Auth0Service(logger, httpClientFactory, cache, authenticationSection);
            var token = await auth0Service.GetCachedTokenAsync(apiName);
            return token;
        }

        private static void AddRegionalHttpClients(
            IServiceCollection services,
            string apiName,
            IConfigurationSection clientConfiguration,
            MultiRegionSettings multiRegionSettings,
            string serviceKey)
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
                    SetServiceKeyHeader(serviceKey, client.DefaultRequestHeaders, clientConfiguration);
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

        private static void SetServiceKeyHeader(string serviceKey, HttpHeaders headers, IConfiguration clientConfiguration)
        {
            if (!string.IsNullOrEmpty(serviceKey) && clientConfiguration.GetValue<bool>("RequiresServiceKey"))
            {
                headers.Add("service-key", serviceKey);
            }
        }
    }
}

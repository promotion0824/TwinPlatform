using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using Willow.Api.AzureStorage;
using Willow.Api.Client;
using Willow.Api.DataValidation;
using Willow.Common;
using Willow.Data;
using Willow.Data.Rest;
using Willow.Infrastructure;
using Willow.Infrastructure.Exceptions;
using Willow.Infrastructure.MultiRegion;
using Willow.Infrastructure.Swagger;
using Willow.Platform.Models;
using Willow.Platform.Statistics;

using PlatformPortalXL.Features;
using PlatformPortalXL.Infrastructure.Swagger;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Polly;
using Polly.Retry;
using Swashbuckle.AspNetCore.SwaggerGen;
using PlatformPortalXL.Features.Pilot;

namespace Willow.PlatformPortalXL
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDigitalApiService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAzureBlobStorage<DigitalTwinService>(configuration)
                    .AddScoped<IDigitalTwinApiService>( p=> new DigitalTwinApiService(p.CreateRestApi(ApiServiceNames.DigitalTwinCore),
                                                                                      p.GetRequiredService<IBlobStore>(),
                                                                                      new FileExtensionContentTypeProvider()));

            return services;
        }

        public static IServiceCollection AddKPIService(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddSiteRepository()
                           //.AddTwinRepository()
                           .AddSingleton<IKPIServiceFactory>( p=> new KPIServiceFactory(configuration,
                                                                                        p.GetRequiredService<IReadRepository<Guid, Site>>(),
                                                                                        p.GetRequiredService<IDigitalTwinApiService>(),
                                                                                        p.GetRequiredService<IMemoryCache>(),
                                                                                        configuration.GetValue<int>("KPICacheDurationInHours", 2),
                                                                                        p.GetRequiredService<ILogger<KPIServiceFactory>>(),
                                                                                        p.GetRequiredService<ILogger<KPIService>>()));
        }

        public static IServiceCollection AddSiteRepository(this IServiceCollection services)
        {
            services.AddSingleton<IReadRepository<Guid, Site>>( (p)=>
            {
                var siteRepo = p.CreateRestApi(ApiServiceNames.SiteCore);
                var cache   = p.GetRequiredService<IMemoryCache>();

                return new CachedRepository<Guid, Site>( new RestRepositoryReader<Guid, Site>(siteRepo, (id)=> $"sites/{id}", null), cache, TimeSpan.FromHours(1), "Site_");
            });

            return services;
        }

        //public static IServiceCollection AddTwinRepository(this IServiceCollection services)
        //{
        //    services.AddSingleton<IReadRepository<string, TwinDto>>((p) =>
        //    {
        //        var twinRepo = p.CreateRestApi(ApiServiceNames.DigitalTwinCore);
        //        var cache = p.GetRequiredService<IMemoryCache>();

        //        return new CachedRepository<string, TwinDto>(new RestRepositoryReader<string, TwinDto>(twinRepo, (dtId) => $"twins/{dtId}", null), cache, TimeSpan.FromHours(1), "Twin_");
        //    });

        //    return services;
        //}

        public static IServiceCollection AddMessageService<TMESSAGESERVICE>(this IServiceCollection services,
            IConfiguration configuration,
            string messageQueue,
            Func<IMessageQueue, IServiceProvider, TMESSAGESERVICE> messageService) where TMESSAGESERVICE : class
        {
            services.AddScoped(p =>
            {
                var connectionString = configuration["ServiceBusConnectionString"] ?? throw new ArgumentException($"Missing ServiceBusConnectionString config entry");
                var queue = configuration[messageQueue];

				var serviceBus = (string.IsNullOrEmpty(queue) ? null : new ServiceBus.ServiceBus(connectionString, queue));

                return messageService(serviceBus, p);
            });

            return services;
        }

        public static IServiceCollection AddStatisticsService(this IServiceCollection services)
        {
            services.AddScoped<IStatisticsService>( (p)=>
            {
                var insightsCore              = p.CreateRestApi(ApiServiceNames.InsightCore);
                var workflowCore              = p.CreateRestApi(ApiServiceNames.WorkflowCore);
                var directoryCore             = p.CreateRestApi(ApiServiceNames.DirectoryCore);
                var cache                     = p.GetRequiredService<IMemoryCache>();
                var ticketRepo                = p.CreateSiteStatsRepo<TicketStats>(workflowCore, cache, "Ticket");
                var portfolioRepo             = new PortfolioInsightsStatsRepository(directoryCore, insightsCore);
                var portfolioInsightsRepo     = new CachedRepository<(Guid, Guid), InsightsStats>(portfolioRepo, cache, TimeSpan.FromMinutes(5), "InsightPortfolioStats_");
                var portfolioTicketRepo       = new PortfolioTicketStatsRepository(directoryCore, workflowCore);
                var cachedPortfolioTicketRepo = new CachedRepository<(Guid, Guid), TicketStats>(portfolioTicketRepo, cache, TimeSpan.FromMinutes(5), "TicketPortfolioStats_");
                var ticketStatsByStatusRepo   = p.CreateSiteTicketStatsByStatusRepo<TicketStatsByStatus>(workflowCore, cache, "TicketByStatus");

                return new StatisticsService(portfolioInsightsRepo, ticketRepo, cachedPortfolioTicketRepo, ticketStatsByStatusRepo);
            });

            return services;
        }

        public static IReadRepository<SiteStatisticsRequest, T> CreateSiteStatsRepo<T>(this IServiceProvider p, IRestApi restApi, IMemoryCache cache, string name)
        {
            var repo  = new RestRepositoryReader<SiteStatisticsRequest, T>(restApi, (r)=>
            {
                return $"statistics/site/{r.SiteId}" + (string.IsNullOrWhiteSpace(r.FloorId) ? "" : $"?floorId={r.FloorId}");
            },
            null);

            return new CachedRepository<SiteStatisticsRequest, T>( repo, cache, TimeSpan.FromMinutes(5), $"{name}SiteStats_");
        }

        public static IReadRepository<Guid, T> CreateSiteTicketStatsByStatusRepo<T>(this IServiceProvider p, IRestApi restApi, IMemoryCache cache, string name)
        {
            var repo = new RestRepositoryReader<Guid, T>(restApi, (r) =>
            {
                return $"ticketStatisticsByStatus/sites/{r}";
            },
            null);

            return new CachedRepository<Guid, T>(repo, cache, TimeSpan.FromMinutes(5), $"{name}SiteStats_");
        }

        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
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
            services.AddMvc(options => {
                options.Filters.Add<RestExceptionFilter>();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = ValidationResponse.Handle;
            })
            .AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                opt.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
            });

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            if (configuration.GetValue<bool>("EnableSwagger"))
            {
                services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

                services.AddSwaggerGen(options =>
                {
                    options.EnableAnnotations();
                    options.CustomSchemaIds(x => x.FullName);

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

                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(System.AppContext.BaseDirectory, xmlFile);
                    options.IncludeXmlComments(xmlPath);
                });
            }

            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddSingleton<IStaleCache, StaleCache>();

            return services;
        }

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

        internal static async Task<string> FetchMachineToMachineToken(string apiName, IServiceProvider services, IConfigurationSection authenticationSection)
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
                            audience,
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
                                logger.LogError(ex, "Failed to get access token. Http status code: {StatusCode} Failed to get ResponseBody. {Message}", response.StatusCode, ex.Message);
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

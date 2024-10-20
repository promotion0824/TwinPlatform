namespace Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Willow.Infrastructure.Exceptions;
using Willow.Infrastructure.Swagger;

internal static class ServiceCollectionExtensions
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        JsonConvert.DefaultSettings = () =>
        {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            settings.Converters.Add(new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() });
            return settings;
        };

        services.AddHttpContextAccessor();

        var httpClientConfiguration = configuration.GetSection("HttpClientFactory");
        foreach (var clientConfiguration in httpClientConfiguration.GetChildren())
        {
            var apiName = clientConfiguration.Key;
            services.AddHttpClient(apiName, (sv, client) =>
            {
                client.BaseAddress = new Uri(clientConfiguration["BaseAddress"]);
                var authenticationSection = clientConfiguration.GetSection("Authentication");
                if (authenticationSection.GetChildren().Count() > 0)
                {
                    if (authenticationSection["Scheme"] == "TokenFromCookie")
                    {
                        string token;
                        var authValue = sv.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.Request?.Headers["Authorization"];
                        if (authValue.HasValue && authValue.Value.Count > 0)
                        {
                            token = authValue.Value[0].Substring("Bearer ".Length);
                        }
                        else
                        {
                            token = sv.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.User?.FindFirst(ClaimTypes.Authentication)?.Value;
                        }

                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }
                    }
                    else if (authenticationSection["Scheme"] == "PassDownAuthorization")
                    {
                        var authValue = sv.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.Request?.Headers["Authorization"];
                        if (authValue.HasValue && authValue.Value.Count > 0)
                        {
                            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authValue.Value[0]);
                        }
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                            authenticationSection["Scheme"],
                            authenticationSection["Parameter"]);
                    }
                }
            });
        }

        services.AddCors();

        var policy = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme,
                "AzureB2C",
                "AzureAD")
            .RequireAuthenticatedUser()
            .Build();

        var domain = configuration["Auth0:Domain"];
        var audience = configuration["Auth0:Audience"];
        var b2cDomain = configuration["AzureB2C:Domain"];
        var b2cInstance = configuration["AzureB2C:Instance"];

        services.AddMvc(options =>
        {
            options.Filters.Add<GlobalExceptionFilter>();
            if ((!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(audience))
                || (!string.IsNullOrEmpty(b2cDomain) && !string.IsNullOrEmpty(b2cInstance)))
            {
                options.Filters.Add(new AuthorizeFilter(policy));
            }
        })

            //Calling SetCompatibilityVersion with any value of CompatibilityVersion has no impact on the application
            //SetCompatibilityVersion is also obsolete
            //https://docs.microsoft.com/en-us/aspnet/core/mvc/compatibility-version?view=aspnetcore-6.0
            //.SetCompatibilityVersion(CompatibilityVersion.Latest)

            /*.AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                opt.JsonSerializerOptions.IgnoreNullValues = true;
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                opt.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                opt.JsonSerializerOptions.Converters.Add(new PolymorphicWriteOnlyJsonConverter<TimeSeriesData>());
                opt.JsonSerializerOptions.Converters.Add(new PolymorphicWriteOnlyJsonConverter<Dictionary<Guid, List<TimeSeriesData>>>());
            })*/
            .AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                opt.SerializerSettings.Converters.Add(new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() });
            });

        if (configuration.GetValue<bool>("EnableSwagger"))
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = Assembly.GetEntryAssembly()?.GetName().Name, Version = "1" });
                options.EnableAnnotations();
                options.CustomSchemaIds(x => x.Name);

                if (configuration.GetValue<string>("Auth0:Domain") != null)
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
                            },
                        },
                    });
                }
            });
        }
    }

    public static void AddApplicationPart(this IServiceCollection services, Assembly assembly)
    {
        var managerService = services.FirstOrDefault(s => s.ServiceType == typeof(ApplicationPartManager));
        if (managerService != null)
        {
            var applicationParts = ((ApplicationPartManager)managerService.ImplementationInstance).ApplicationParts;
            var exists = applicationParts.Any(p => (p is AssemblyPart) ? ((AssemblyPart)p).Assembly == assembly : false);
            if (!exists)
            {
                var part = new AssemblyPart(assembly);
                applicationParts.Add(part);
            }
        }
    }

    public static IServiceCollection ReplaceSingleton<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
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

    public static IServiceCollection ReplaceScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
        where TService : class
    {
        var service = services.First(s => s.ServiceType == typeof(TService));
        services.Remove(service);
        return services.AddScoped(implementationFactory);
    }

    public static IServiceCollection ReplaceScoped<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        var service = services.First(s => s.ServiceType == typeof(TService));
        services.Remove(service);
        return services.AddScoped<TService, TImplementation>();
    }
}

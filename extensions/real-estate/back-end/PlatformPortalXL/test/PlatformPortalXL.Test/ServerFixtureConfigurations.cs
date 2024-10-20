using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Willow.Tests.Infrastructure;
using PlatformPortalXL.Services;
using PlatformPortalXL.Test.MockServices;
using PlatformPortalXL.Services.Forge;
using PlatformPortalXL.Services.PowerBI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Authorization.TwinPlatform.Common.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Infrastructure.AppSettingOptions;
using PlatformPortalXL.Services.ArcGis;
using PlatformPortalXL.Services.CognitiveSearch;
using Willow.PlatformPortalXL;
using Willow.Common;
using Willow.MessageDispatch;
using PlatformPortalXL.Services.GeometryViewer;
using PlatformPortalXL.ServicesApi.ZendeskApi;
using PlatformPortalXL.ServicesApi.WeatherbitApi;
using Willow.Notifications.Interfaces;
using PlatformPortalXL.Services.Copilot;
using PlatformPortalXL.Services.SiteWeatherCache;

namespace PlatformPortalXL.Test
{
    public class ServerFixtureConfigurations
    {
        public static readonly Guid RulingEngineAppId = Guid.NewGuid();
        public static readonly Guid RulesEngineConnectorId = Guid.NewGuid();

        public static readonly ServerFixtureConfiguration Default = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                var authorizationApiSettings = GetAuthorizationApiSettings();
                var fixtureSettings = new Dictionary<string, string>
                {
                    ["RulingEngineAppId"] = RulingEngineAppId.ToString(),
                    ["SigmaOptions:SessionLength"] = "3600",
                    ["SigmaOptions:AccountType"] = "Embedded_Basic_Explore_Access",
                    ["SigmaOptions:Theme"] = "Willow Theme",
                    ["SigmaOptions:Mode"] = "userbacked",
                    ["SigmaOptions:ClientId"] = "secret",
                    ["SigmaOptions:Team"] = "EMBED_GROUP",
                    ["SigmaOptions:EmbedSecret"] = "secret",
                    ["WillowUserEmailDomain"] = "willowinc.com",
                    ["RulesEngineConnectorId"] = RulesEngineConnectorId.ToString()
                };
                var combinedSettings = authorizationApiSettings.Concat(fixtureSettings).ToDictionary(p => p.Key, p => p.Value);
                configuration.AddInMemoryCollection(combinedSettings);
            },
            MainServicePostConfigureServices = (services) =>
            {
                services.ReplaceScoped<IGeometryViewerMessagingService, MockGeometryViewerMessagingService>();
                services.ReplaceSingleton<IHostedMessageDispatch, MockMessageDispatch>();
                services.AddSingleton<IPowerBIClientFactory, MockPowerBIClientFactory>();
                services.ReplaceScoped<IForgeApi, MockForgeApi>();
                services.ReplaceScoped<IArcGisService, MockArcGisService>();
                services.ReplaceScoped<IZendeskApiService, MockZendeskApiService>();
                services.ReplaceScoped<ICopilotService, MockCopilotService>();
                services.ReplaceScoped<IMessageQueue, MockMessageQueue>();
                services.ReplaceScoped<INotificationService>(_ => new MockNotificationService());
                services.ReplaceScoped<IWeatherbitApiService, MockWeatherbitApiService>();
                services.AddTransient<IUserAuthorizationService, MockUserAuthorizationService>();
                services.ReplaceSingleton<IAncestralTwinsSearchService, MockAncestralTwinsSearchService>();
                // When running integration tests we don't need to initialise the cache.
                services.Remove(services.Single(s => s.ImplementationType == typeof(SiteWeatherCacheHostedService)));
                services.Remove(services.Single(s => s.ImplementationType == typeof(AncestralTwinsCacheUpdateHostedService)));
                services.Remove(services.Single(s => s.ImplementationType == typeof(UserManagementImportHostedService)));
                services.Remove(services.Single(s => s.ImplementationType == typeof(SiteTwinCacheUpdateHostedService)));
                services.AddSingleton<IOptions<AppSettings>>(x => Options.Create(new AppSettings{ZendeskOptions = new ZendeskOptions{ RequesterName= "Command Portal",AuthUsername = "blah@zendesk.com" } } ));
                services.AddSingleton<IOptions<ArcGisOptions>>(x => Options.Create(new ArcGisOptions { CustomerId = Guid.Parse("35a800f0-67a4-4e24-ba63-cb7e0d485791") }));
                services.AddSingleton<IOptions<ForgeOptions>>((sp) => Options.Create(new ForgeOptions { ClientId = "forgeclientid", ClientSecret = string.Empty }));
                services.AddSingleton<IOptions<AzureCognitiveSearchOptions>>(x => Options.Create(new AzureCognitiveSearchOptions { IndexName = "mock-index", Uri = "https://mock-uri" }));
                services.AddAuthorization(options =>
                {
                    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        JwtBearerDefaults.AuthenticationScheme,
                        "AzureB2C",
                        "AzureAD",
                        "TestScheme");
                    defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
                });
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DirectoryCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.SiteCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.WorkflowCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.InsightCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.AssetCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ConnectorCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DigitalTwinCore
                },
				new DependencyServiceConfiguration
				{
					Name = ApiServiceNames.ArcGis
				},
				new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.LiveDataCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.MarketPlaceCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ConnectorImporter
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ConnectorExporter
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.NotificationCore
                }
            }
        };

        public static readonly ServerFixtureConfiguration DefaultWithSingleTenantOption = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                var settings = new Dictionary<string, string>
                {
                    ["RulingEngineAppId"] = RulingEngineAppId.ToString(),
                    ["SigmaOptions:SessionLength"] = "3600",
                    ["SigmaOptions:AccountType"] = "Embedded_Basic_Explore_Access",
                    ["SigmaOptions:Theme"] = "Willow Theme",
                    ["SigmaOptions:Mode"] = "userbacked",
                    ["SigmaOptions:ClientId"] = "secret",
                    ["SigmaOptions:Team"] = "EMBED_GROUP",
                    ["SigmaOptions:EmbedSecret"] = "secret",
                    ["WillowUserEmailDomain"] = "willowinc.com",
                    ["RulesEngineConnectorId"] = RulesEngineConnectorId.ToString(),
                    ["AuthorizationAPI:BaseAddress"] = "http://authrz-api/",
                    ["AuthorizationAPI:TokenAudience"] = "api://8c74e778-9a35-473b-bbf0-4b0e9cc6000e",
                    ["AuthorizationAPI:APITimeoutMilliseconds"] = "25000",
                    ["AuthorizationAPI:ExtensionName"] = "WillowApp",
                    ["AuthorizationAPI:InstanceType"] = "nonprd",
                    ["SingleTenantOptions:AdminPermission"] = "CanActAsCustomerAdmin",
                    ["SingleTenantOptions:CustomerUserIdForGroupUser"] = "4EE803FB-2F44-4FCD-9ED1-EEE836C487CD"
                };
                configuration.AddInMemoryCollection(settings);
            },
            MainServicePostConfigureServices = (services) =>
            {
                services.ReplaceScoped<IGeometryViewerMessagingService, MockGeometryViewerMessagingService>();
                services.ReplaceSingleton<IHostedMessageDispatch, MockMessageDispatch>();
                services.AddSingleton<IPowerBIClientFactory, MockPowerBIClientFactory>();
                services.ReplaceScoped<IForgeApi, MockForgeApi>();
                services.ReplaceScoped<IArcGisService, MockArcGisService>();
                services.ReplaceScoped<IZendeskApiService, MockZendeskApiService>();
                services.ReplaceScoped<ICopilotService, MockCopilotService>();
                services.ReplaceScoped<IMessageQueue, MockMessageQueue>();
                services.ReplaceScoped<INotificationService>(p => new MockNotificationService());
                services.ReplaceScoped<IWeatherbitApiService, MockWeatherbitApiService>();
                services.ReplaceScoped<IUserAuthorizationService, MockUserAuthorizationService>();
                // When running integration tests we don't need to initialise the cache.
                services.Remove(services.Single(s => s.ImplementationType == typeof(SiteWeatherCacheHostedService)));
                services.Remove(services.Single(s => s.ImplementationType == typeof(AncestralTwinsCacheUpdateHostedService)));
                services.Remove(services.Single(s => s.ImplementationType == typeof(UserManagementImportHostedService)));
                services.Remove(services.Single(s => s.ImplementationType == typeof(SiteTwinCacheUpdateHostedService)));
                services.AddSingleton<IOptions<AppSettings>>(x => Options.Create(new AppSettings { ZendeskOptions = new ZendeskOptions { RequesterName = "Command Portal", AuthUsername = "blah@zendesk.com" } }));
                services.AddSingleton<IOptions<ArcGisOptions>>(x => Options.Create(new ArcGisOptions { CustomerId = Guid.Parse("35a800f0-67a4-4e24-ba63-cb7e0d485791") }));
                services.AddSingleton<IOptions<ForgeOptions>>((sp) => Options.Create(new ForgeOptions { ClientId = "forgeclientid", ClientSecret = string.Empty }));
                services.AddSingleton(_ => Options.Create(new AzureCognitiveSearchOptions { IndexName = "mock-index", Uri = "https://mock-uri" }));
                services.AddAuthorization(options =>
                {
                    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        JwtBearerDefaults.AuthenticationScheme,
                        "AzureB2C",
                        "AzureAD",
                        "TestScheme");
                    defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
                });
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DirectoryCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.SiteCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.WorkflowCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.InsightCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.AssetCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ConnectorCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DigitalTwinCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ArcGis
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.LiveDataCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.MarketPlaceCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ConnectorImporter
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ConnectorExporter
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.NotificationCore
                }
            }
        };

        public static readonly ServerFixtureConfiguration DefaultWithoutAdditionalSigmaConfig = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                var authorizationApiSettings = GetAuthorizationApiSettings();
                var fixtureSettings = new Dictionary<string, string>
                {
                    ["RulingEngineAppId"] = RulingEngineAppId.ToString(),
                    ["SigmaOptions:SessionLength"] = "3600",
                    ["SigmaOptions:EmbedSecret"] = "secret",
                    ["SigmaOptions:Mode"] = "[value from app service setting]",
                    ["WillowUserEmailDomain"] = "willowinc.com",
                    ["RulesEngineConnectorId"] = RulesEngineConnectorId.ToString()
                };
                var combinedSettings = authorizationApiSettings.Concat(fixtureSettings).ToDictionary(p => p.Key, p => p.Value);
                configuration.AddInMemoryCollection(combinedSettings);
            },
            MainServicePostConfigureServices = services =>
            {
                services.ReplaceScoped<IGeometryViewerMessagingService, MockGeometryViewerMessagingService>();
                services.ReplaceSingleton<IHostedMessageDispatch, MockMessageDispatch>();
                services.Remove(services.Single(s => s.ImplementationType == typeof(SiteWeatherCacheHostedService)));
                services.AddAuthorization(options =>
                {
                    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        JwtBearerDefaults.AuthenticationScheme,
                        "AzureB2C",
                        "AzureAD",
                        "TestScheme");
                    defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
                });
                services.AddTransient<IUserAuthorizationService, MockUserAuthorizationService>();
                services.Remove(services.Single(s => s.ImplementationType == typeof(AncestralTwinsCacheUpdateHostedService)));
                services.Remove(services.Single(s => s.ImplementationType == typeof(UserManagementImportHostedService)));
                services.Remove(services.Single(s => s.ImplementationType == typeof(SiteTwinCacheUpdateHostedService)));
                services.AddSingleton(_ => Options.Create(new AzureCognitiveSearchOptions { IndexName = "mock-index", Uri = "https://mock-uri" }));
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DirectoryCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.SiteCore
                }
            }
        };

        public static readonly ServerFixtureConfiguration DefaultWithoutTestAuthentication = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = false,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                var authorizationApiSettings = GetAuthorizationApiSettings();
                var fixtureSettings = new Dictionary<string, string>
                {
                    ["SingleTenantOptions:CustomerUserIdForGroupUser"] = "4EE803FB-2F44-4FCD-9ED1-EEE836C487CD"
                };
                var combinedSettings = authorizationApiSettings.Concat(fixtureSettings).ToDictionary(p => p.Key, p => p.Value);
                configuration.AddInMemoryCollection(combinedSettings);
            },
            MainServicePostConfigureServices = services =>
            {
                services.ReplaceScoped<IGeometryViewerMessagingService, MockGeometryViewerMessagingService>();
                services.ReplaceSingleton<IHostedMessageDispatch, MockMessageDispatch>();
                services.AddTransient<IUserAuthorizationService, MockUserAuthorizationService>();
                services.Remove(services.Single(s => s.ImplementationType == typeof(SiteWeatherCacheHostedService)));
                services.Remove(services.Single(s => s.ImplementationType == typeof(AncestralTwinsCacheUpdateHostedService)));
                services.Remove(services.Single(s => s.ImplementationType == typeof(UserManagementImportHostedService)));
                services.Remove(services.Single(s => s.ImplementationType == typeof(SiteTwinCacheUpdateHostedService)));
                services.AddSingleton(_ => Options.Create(new AzureCognitiveSearchOptions { IndexName = "mock-index", Uri = "https://mock-uri" }));
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DirectoryCore
                }
            }
        };

        private static Dictionary<string, string> GetAuthorizationApiSettings()
        {
            return new Dictionary<string, string>
            {
                ["AuthorizationAPI:BaseAddress"] = "http://authrz-api-test/",
                ["AuthorizationAPI:TokenAudience"] = "api://4aa74269-b33e-4659-ae57-9fbff2743f27",
                ["AuthorizationAPI:APITimeoutMilliseconds"] = "2000",
                ["AuthorizationAPI:ExtensionName"] = "WillowAppTest",
                ["AuthorizationAPI:InstanceType"] = "test",
                ["AuthorizationAPI:CacheExpiration"] = "0.00:02:00"
            };
        }
    }
}

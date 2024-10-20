using Autofac;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Api.Services.Domain.Instance.Readers;
using Willow.AzureDigitalTwins.Services.Cache.Providers;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Domain.InMemory.Readers;
using Willow.AzureDigitalTwins.Services.Domain.InMemory.Writers;
using Willow.AzureDigitalTwins.Services.Domain.Instance.Readers;
using Willow.AzureDigitalTwins.Services.Domain.Instance.Writers;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.AzureDigitalTwins.Services.Parser;
using Willow.AzureDigitalTwins.Services.Storage;
using Willow.CognitiveSearch.Extensions;

namespace Willow.AzureDigitalTwins.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static void RegisterDigitalTwinsService(this ContainerBuilder containerBuilder, AzureDigitalTwinsSettings azureDigitalTwinsSettings)
    {
        containerBuilder.RegisterType<AzureDigitalTwinModelParser>().As<IAzureDigitalTwinModelParser>();
        containerBuilder.RegisterType<TwinValidatorService>().As<IAzureDigitalTwinValidator>();   
        containerBuilder.Register<IAzureDigitalTwinCacheProvider>(x =>
        {
            if (azureDigitalTwinsSettings.SourceType == AzureDigitalTwinSourceType.Instance)
                return new EmptyCacheProvider(azureDigitalTwinsSettings.InMemory, x.Resolve<IMemoryCache>(), x.Resolve<ILogger<EmptyCacheProvider>>(), new AzureDigitalTwinReader(azureDigitalTwinsSettings.Instance, x.Resolve<TokenCredential>(), x.Resolve<ILogger<AzureDigitalTwinReader>>()));

            if (azureDigitalTwinsSettings.InMemory.Source == InMemorySourceType.LocalSystem)
                return new LocalFilesCacheProvider(azureDigitalTwinsSettings.InMemory, x.Resolve<IStorageReader>(), x.Resolve<IMemoryCache>(), x.Resolve<ILogger<LocalFilesCacheProvider>>());

            if (azureDigitalTwinsSettings.InMemory.Source == InMemorySourceType.Instance && azureDigitalTwinsSettings.InMemory.Lazy)
                return new LazyInstanceCacheProvider(azureDigitalTwinsSettings.InMemory, x.Resolve<IMemoryCache>(), x.Resolve<ILogger<LazyInstanceCacheProvider>>(), new AzureDigitalTwinReader(azureDigitalTwinsSettings.Instance, x.Resolve<TokenCredential>(), x.Resolve<ILogger<AzureDigitalTwinReader>>()));

            if (azureDigitalTwinsSettings.InMemory.Source == InMemorySourceType.Instance)
                return new InstanceCacheProvider(azureDigitalTwinsSettings.InMemory, x.Resolve<IMemoryCache>(), x.Resolve<ILogger<InstanceCacheProvider>>(), new AzureDigitalTwinReader(azureDigitalTwinsSettings.Instance, x.Resolve<TokenCredential>(), x.Resolve<ILogger<AzureDigitalTwinReader>>()));

            throw new Exception("Invalid in memory settings");

        }).As<IAzureDigitalTwinCacheProvider>();

        containerBuilder.Register(x =>
        {
            var map = new Dictionary<AzureDigitalTwinSourceType, Func<IAzureDigitalTwinReader>>
            {
                {
                    AzureDigitalTwinSourceType.Instance, () =>
                        new AzureDigitalTwinCacheReader(
                            azureDigitalTwinsSettings.Instance,
                            x.Resolve<TokenCredential>(),
                            x.Resolve<ILogger<AzureDigitalTwinCacheReader>>(),
                            x.Resolve<IAzureDigitalTwinCacheProvider>(),
                            x.Resolve<IAzureDigitalTwinModelParser>())
                },
                {
                    AzureDigitalTwinSourceType.InMemory, () =>
                    {
                        if(azureDigitalTwinsSettings.InMemory.Source == InMemorySourceType.Instance && azureDigitalTwinsSettings.InMemory.Lazy)
                            return new InMemoryInstanceLazyTwinReader(x.Resolve<IAzureDigitalTwinModelParser>(),
                                x.Resolve<IAzureDigitalTwinCacheProvider>(),
                                new AzureDigitalTwinReader(azureDigitalTwinsSettings.Instance, x.Resolve<TokenCredential>(), x.Resolve<ILogger<AzureDigitalTwinReader>>()));

                        if(azureDigitalTwinsSettings.InMemory.Source == InMemorySourceType.Instance)
                            return new InMemoryInstanceTwinReader(new AzureDigitalTwinReader(azureDigitalTwinsSettings.Instance, x.Resolve<TokenCredential>(), x.Resolve<ILogger<AzureDigitalTwinReader>>()),
                                x.Resolve<IAzureDigitalTwinModelParser>(),
                                x.Resolve<IAzureDigitalTwinCacheProvider>());

                        if(azureDigitalTwinsSettings.InMemory.Source == InMemorySourceType.LocalSystem)
                            return new InMemoryTwinReader(x.Resolve<IAzureDigitalTwinModelParser>(), x.Resolve<IAzureDigitalTwinCacheProvider>());

                        throw new Exception("Invalid in memory settings");
                    }
                }
            };

            if (!map.ContainsKey(azureDigitalTwinsSettings.SourceType))
                throw new Exception("Invalid azure digital twin source type configuration");

            return map[azureDigitalTwinsSettings.SourceType]();
        }).As<IAzureDigitalTwinReader>();

        containerBuilder.Register(x =>
        {
            var map = new Dictionary<AzureDigitalTwinSourceType, Func<IAzureDigitalTwinWriter>>
            {
                {
                    AzureDigitalTwinSourceType.Instance, () => new InMemoryInstanceTwinWriter(
                                x.Resolve<IAzureDigitalTwinModelParser>(),
                                x.Resolve<IAzureDigitalTwinCacheProvider>(),
                                new AzureDigitalTwinWriter(azureDigitalTwinsSettings.Instance,
                                x.Resolve<TokenCredential>(),
                                x.Resolve<IAzureDigitalTwinModelParser>(),
                                x.Resolve<ILogger<AzureDigitalTwinWriter>>(),
                                x.Resolve<IEnumerable<IAzureDigitalTwinValidator>>()))
                },
                {
                    AzureDigitalTwinSourceType.InMemory, () =>
                    {
                        if(azureDigitalTwinsSettings.InMemory.Sync)
                            return new InMemoryInstanceTwinWriter(
                                x.Resolve<IAzureDigitalTwinModelParser>(),
                                x.Resolve<IAzureDigitalTwinCacheProvider>(),
                                new AzureDigitalTwinWriter(azureDigitalTwinsSettings.Instance,
                                x.Resolve<TokenCredential>(), x.Resolve<IAzureDigitalTwinModelParser>(),
                                x.Resolve<ILogger<AzureDigitalTwinWriter>>(),
                                x.Resolve<IEnumerable<IAzureDigitalTwinValidator>>()));

                        return new InMemoryTwinWriter(x.Resolve<IAzureDigitalTwinModelParser>(), x.Resolve<IAzureDigitalTwinCacheProvider>());
                    }
                }
            };

            if (!map.ContainsKey(azureDigitalTwinsSettings.SourceType))
                throw new Exception("Invalid azure digital twin source type configuration");

            return map[azureDigitalTwinsSettings.SourceType]();
        }).As<IAzureDigitalTwinWriter>();

        /*
				Commenting as currently we do not require scaling and this might generate unnecessary extra processing
				Might need to revisit and enable when scaling is required

			containerBuilder.Register(x => new SyncCacheMessageHandler(
				x.Resolve<IOptions<CacheSyncTopic>>(),
				x.Resolve<IAzureDigitalTwinCacheProvider>(),
				new AzureDigitalTwinReader(azureDigitalTwinsSettings.Instance, x.Resolve<TokenCredential>()),
				x.Resolve<ILogger<SyncCacheMessageHandler>>())).As<ITopicMessageHandler>();

			To create the cache sync subscription with TPD, add the following to the appSettings & similar manifest:

			  "CacheSyncTopic": {
				"ServiceBusName": "CustomerServiceBus",
				"TopicName": "adt-sync",
				"SubscriptionName": "cache-sync-subscription"
			  },
			*/
    }

    public static void RegisterCustomFormatters(this IServiceCollection services)
    {
        services.AddControllers().AddNewtonsoftJson().AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        services.AddOptions<MvcOptions>()
            .PostConfigure<IOptions<JsonOptions>, IOptions<MvcNewtonsoftJsonOptions>, ArrayPool<char>, ObjectPoolProvider, ILoggerFactory>((opts, jsonOpts, newtonJsonOpts, charPool, objectPoolProvider, loggerFactory) =>
            {
                //Make sure inclue SystemTextJsonOutputFormatter since it is removed by addnewtonsoft
                if (opts.OutputFormatters.OfType<SystemTextJsonOutputFormatter>().Count() == 0)
                {
                    opts.OutputFormatters.Insert(0, new SystemTextJsonOutputFormatter(jsonOpts.Value.JsonSerializerOptions));
                }

                if (opts.InputFormatters.OfType<SystemTextJsonInputFormatter>().Count() == 0)
                {
                    var systemInputlogger = loggerFactory.CreateLogger<SystemTextJsonInputFormatter>();
                    opts.InputFormatters.Insert(0, new SystemTextJsonInputFormatter(jsonOpts.Value, systemInputlogger));
                }
            });
    }

    public static void RegisterFilesReader(this ContainerBuilder containerBuilder, AzureDigitalTwinsSettings azureDigitalTwinsSettings)
    {
        containerBuilder.Register<IStorageReader>(x =>
        {
            if (azureDigitalTwinsSettings.InMemory.LocalSystem.Zipped)
                return new ZipFilesReader(azureDigitalTwinsSettings.InMemory.LocalSystem);

            return new LocalFilesReader(azureDigitalTwinsSettings.InMemory.LocalSystem);
        }).As<IStorageReader>();
    }

    public static void AddAISearchCapabilities(this IServiceCollection services, string searchConfigSectionName)
    {
        services.AddScoped<IAcsService, AcsService>();

        // Configure ACS Document Indexer Settings
        services.AddOptions<AISearchIndexerSetupOption>().BindConfiguration(AISearchIndexerSetupOption.Name);
        services.AddOptions<AzureOpenAIServiceOption>().BindConfiguration(AzureOpenAIServiceOption.Name);
        services.AddSingleton<IAISearchIndexerSetupService,AISearchIndexerSetupService>();

        // Add Willow Cognitive Search library
        services.AddAIIndexerBuildServices(searchConfigSectionName);
    }
}

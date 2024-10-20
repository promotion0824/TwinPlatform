using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using Azure.Identity;
using System.Diagnostics;
using Azure;
using System.Net;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.BackupRestore.Log;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Storage;
using Willow.AzureDigitalTwins.Services.Domain.Instance.Writers;
using Willow.AzureDigitalTwins.Services.Domain.Instance.Readers;
using Azure.Core;
using Willow.AzureDigitalTwins.Services.Parser;
using Willow.AzureDigitalTwins.Services.Cache.Providers;
using Microsoft.Extensions.Logging;

namespace Willow.AzureDigitalTwins.BackupRestore
{
    public class Program
    {
        private static ServiceProvider _serviceProvider;

        static void Main(string[] args)
        {
			var parser = new Parser(settings => settings.CaseSensitive = false);

            parser.ParseArguments<Options>(args)
                .WithParsed(x =>
                {
                    var process = RunOptions(x);
                    process.Wait();
                })
                .WithNotParsed(HandleParseError);
        }

        private static async Task<bool> EnsureAdtInstaceConnectivity(Options options, IInteractiveLogger interactiveLogger)
        {
            var adtService = _serviceProvider.GetRequiredService<AzureDigitalTwinReader>();
            try
            {
                interactiveLogger.LogLine($"Testing connectivity to {options.AdtInstance}...");
                var twins = await adtService.GetTwinsCountAsync();
                interactiveLogger.LogLine($"Done testing connectivity to {options.AdtInstance}...");
            }
            catch (RequestFailedException requestFailedException)
            {
                interactiveLogger.SetErrorFormat();
                if (requestFailedException.Status == (int)HttpStatusCode.Forbidden || requestFailedException.Status == (int)HttpStatusCode.Unauthorized)
                {
                    interactiveLogger.LogLine($"Make sure you have the appropriate permissions to connect to {options.AdtInstance}");
                }
                if (requestFailedException.Status == (int)HttpStatusCode.NotFound)
                {
                    interactiveLogger.LogLine($"Make sure you have the appropriate adt instance name {options.AdtInstance}");
                }
                interactiveLogger.LogLine($"Service response: {requestFailedException.Message}");
                return false;
            }
            catch (Exception ex)
            {
                interactiveLogger.SetErrorFormat();
                interactiveLogger.LogLine($"Failed attemping to connect to ADT instance {options.AdtInstance}: {ex.InnerException.Message}");
                return false;
            }
            return true;
        }

        private static async Task RunOptions(Options options)
        {
            RegisterServices(options);

            var interactiveLogger = _serviceProvider.GetRequiredService<IInteractiveLogger>();

            var uriString = $"https://{options.AdtInstance}";
            if (!Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
            {
                interactiveLogger.SetErrorFormat();
                interactiveLogger.LogLine("Please make sure you provide only the host name as shown in azure portal");
                return;
            }
            
            var connectivityResult = await EnsureAdtInstaceConnectivity(options, interactiveLogger);
            if (!connectivityResult)
                return;

            var processor = _serviceProvider.GetRequiredService<IProcessorFactory>().GetProcessor(_serviceProvider);
            if (processor == null)
                return;

            var validationErros = processor.GetValidationErrors();
            if (validationErros.Any())
            {
                interactiveLogger.SetErrorFormat();
                validationErros.ForEach(x => interactiveLogger.LogLine(x));
				interactiveLogger.ResetFormat();
                return;
            }
            var watch = new Stopwatch();
            watch.Start();

            var result = await processor.ProcessCommand();

            watch.Stop();

            interactiveLogger.LogLine($"Elapsed time: {Math.Round(watch.Elapsed.TotalMinutes)} minutes.");

            result.GenerateOutput();

            interactiveLogger.ResetFormat();
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            if (errs.All(x => x.Tag == ErrorType.VersionRequestedError || x.Tag == ErrorType.HelpRequestedError))
                return;

            Console.WriteLine($"The following errors were found: {string.Join(", ", errs)}");
        }

        private static void RegisterServices(Options options)
        {
            var services = new ServiceCollection();
			var logger = _serviceProvider.GetRequiredService<ILogger<AzureDigitalTwinReader>>();

			services.AddMemoryCache();

			var settings = new InstanceSettings { InstanceUri = options.InstanceUri };
			var memorySettings = new InMemorySettings { Source = InMemorySourceType.Empty };

			services.AddSingleton(typeof(TokenCredential), new DefaultAzureCredential(includeInteractiveCredentials: true));
			services.AddSingleton(typeof(AzureDigitalTwinReader), new AzureDigitalTwinReader(settings, new DefaultAzureCredential(includeInteractiveCredentials: true), logger));
			services.AddSingleton<IAzureDigitalTwinReader, AzureDigitalTwinReader>();
			services.AddSingleton<IAzureDigitalTwinWriter, AzureDigitalTwinWriter>();
			services.AddSingleton<IAzureDigitalTwinModelParser, AzureDigitalTwinModelParser>();
			services.AddSingleton<IAzureDigitalTwinCacheProvider, EmptyCacheProvider>();
			services.AddSingleton<IProcessorFactory, ProcessorFactory>();
			services.AddSingleton<IInteractiveLogger, InteractiveLogger>();
			services.AddSingleton<IStorageReader, LocalFilesReader>();
			services.AddMemoryCache();
			services.AddSingleton(typeof(Options), options);
			services.AddLogging();
			services.AddSingleton(typeof(InstanceSettings), settings);
			services.AddSingleton(typeof(InMemorySettings), memorySettings);
			services.AddSingleton(typeof(LocalSystemSettings), new LocalSystemSettings { Path = options.ImportSource, Zipped = options.Zipped });
            
            _serviceProvider = services.BuildServiceProvider(true);
        }
    }
}

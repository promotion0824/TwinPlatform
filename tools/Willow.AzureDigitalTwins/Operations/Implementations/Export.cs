using System;
using System.Threading.Tasks;
using Azure;
using Azure.DigitalTwins.Core;
using System.IO;
using System.Text.Json;
using Willow.AzureDigitalTwins.BackupRestore.Results;
using System.Collections.Generic;
using Willow.AzureDigitalTwins.BackupRestore.Log;
using Willow.AzureDigitalTwins.BackupRestore.Models;
using System.Linq;
using Willow.AzureDigitalTwins.Services.Domain.Instance.Readers;
using Willow.AzureDigitalTwins.BackupRestore.Operations.Base;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.BackupRestore.Operations.Implementations
{
    public class Export : CommandRunner
	{
        protected readonly AzureDigitalTwinReader _azureDigitalTwinService;
        protected readonly Options _settings;
        protected readonly ExportResult _exportResult;
        protected readonly IInteractiveLogger _interactiveLogger;

        public Export(AzureDigitalTwinReader azureDigitalTwinService, Options settings, IInteractiveLogger interactiveLogger)
        {
            _azureDigitalTwinService = azureDigitalTwinService;
            _settings = settings;
            _exportResult = new ExportResult(_settings, interactiveLogger);
            _interactiveLogger = interactiveLogger;
        }

        public override List<string> GetValidationErrors()
        {
            var isValid = true;
            var errors = new List<string>();
            if (string.IsNullOrEmpty(_settings.OutputDirectory))
            {
                errors.Add("Please provide a valid output directory.");
                isValid = false;
            }

            if (isValid && !Directory.Exists(_settings.OutputDirectory))
            {
                errors.Add("The provided output directory does not exist.");
            }
            return errors;
        }

        public override async Task<ProcessResult> ProcessCommand()
        {
            var adtInstanceDirectory = Path.Combine(_settings.OutputDirectory, _settings.InstanceUri.Host);
            if (!Directory.Exists(adtInstanceDirectory))
            {
                Directory.CreateDirectory(adtInstanceDirectory);
            }

            _interactiveLogger.LogLine($"Export triggered for adt instance {_settings.AdtInstance}.");

            var currentRunTimeString = DateTime.Now.ToString("yyyy.MM.dd.HH.mm");
            var currentRunDirectory = Path.Combine(adtInstanceDirectory, currentRunTimeString);
            CreateDirectories(currentRunDirectory);

            int? sourceModelsCount = null;
            if (_settings.IncludeModels)
            {
				_interactiveLogger.LogLine("Retrieving models...");

				var sourceModels = await _azureDigitalTwinService.GetModelsAsync();
                sourceModelsCount = sourceModels.Count();

                var models = AsyncPageable<DigitalTwinsModelBasicData>.FromPages(new List<Azure.Page<DigitalTwinsModelBasicData>> { Azure.Page<DigitalTwinsModelBasicData>.FromValues(sourceModels.ToList().AsReadOnly(), null, null) });
				
                _exportResult.ExportedModelsCount = await ProcessPageable<DigitalTwinsModelBasicData>(models, (entities, pageNum) => ProcessModels(entities, pageNum, currentRunDirectory));

                _interactiveLogger.NewLine();
                _interactiveLogger.LogLine($"Done retrieving models, retrieved {_exportResult.ExportedModelsCount} models...");
            }

            var twins = _azureDigitalTwinService.GetPageableTwins();

            _interactiveLogger.LogLine("Retrieving twins...");

            var sourceTwinsCount = await _azureDigitalTwinService.GetTwinsCountAsync();
            _exportResult.ExportedTwinsCount = await ProcessPageable<BasicDigitalTwin>(twins, (entities, pageNum) => ProcessTwins(entities, pageNum, currentRunDirectory));

            _interactiveLogger.NewLine();
            _interactiveLogger.LogLine($"Done retrieving twins, retrieved {_exportResult.ExportedTwinsCount} twins...");

            _interactiveLogger.LogLine("Retrieving relationships...");

            var relationships = _azureDigitalTwinService.GetPageableRelationships();

            var sourceRelationshipsCount = await _azureDigitalTwinService.GetRelationshipsCountAsync();
            _exportResult.ExportedRelationshipsCount = await ProcessPageable<BasicRelationship>(relationships, (entities, pageNum) => ProcessRelationships(entities, pageNum, currentRunDirectory));

            _interactiveLogger.NewLine();
            _interactiveLogger.LogLine($"Done retrieving relationships, retrieved {_exportResult.ExportedRelationshipsCount} relationships...");            

            await CreateSummaryFile(currentRunDirectory, sourceTwinsCount, sourceRelationshipsCount, sourceModelsCount);

            if (_settings.Zipped)
                _exportResult.ZipFile = Path.Combine(adtInstanceDirectory, $"{currentRunTimeString}.zip");

            _exportResult.OutputDirectory = currentRunDirectory;

            return _exportResult;
        }

        private async Task CreateSummaryFile(string currentRunDirectory, int sourceTwinsCount, int sourceRelationshipsCount, int? sourceModelsCount)
        {
            await File.WriteAllTextAsync(Path.Combine(currentRunDirectory, "summary.json"),
                            JsonSerializer.Serialize(new
                            {
                                Export = new ExportSummary
                                {
                                    RelationshipsCount = _exportResult.ExportedRelationshipsCount,
                                    TwinsCount = _exportResult.ExportedTwinsCount,
                                    ModelsCount = sourceModelsCount
                                },
                                Source = new ExportSummary
                                {
                                    RelationshipsCount = sourceRelationshipsCount,
                                    TwinsCount = sourceTwinsCount,
                                    ModelsCount = _settings.IncludeModels ? (int?)_exportResult.ExportedModelsCount : null
                                }
                            }));
        }

        private async Task CreateFilePage<T>(IReadOnlyList<T> entities, int pageNumber, string currentRunDirectory, string entityFolder)
        {
            await File.WriteAllTextAsync(Path.Combine(currentRunDirectory, entityFolder, $"{pageNumber}.json"), JsonSerializer.Serialize(entities));
        }

        protected virtual async Task ProcessModels(IReadOnlyList<DigitalTwinsModelBasicData> entities, int pageNumber, string currentRunDirectory)
        {
            await CreateFilePage<DigitalTwinsModelBasicData>(entities, pageNumber, currentRunDirectory, Constants.ModelsFolder);
        }

        protected virtual async Task ProcessTwins(IReadOnlyList<BasicDigitalTwin> entities, int pageNumber, string currentRunDirectory)
        {
            await CreateFilePage<BasicDigitalTwin>(entities, pageNumber, currentRunDirectory, Constants.TwinsFolder);
        }

        protected virtual async Task ProcessRelationships(IReadOnlyList<BasicRelationship> entities, int pageNumber, string currentRunDirectory)
        {
            await CreateFilePage<BasicRelationship>(entities, pageNumber, currentRunDirectory, Constants.RelationshipsFolder);
        }

        private async Task<int> ProcessPageable<T>(AsyncPageable<T> asyncPageable, Func<IReadOnlyList<T>, int, Task> processEntities)
        {
            var pageNum = 0;
            var entityCount = 0;

            await foreach (Azure.Page<T> page in asyncPageable.AsPages())
            {
                pageNum++;

                _interactiveLogger.Log($"Reading page {pageNum}...", true);

                await processEntities(page.Values, pageNum);

                entityCount += page.Values.Count;
            }

            return entityCount;
        }

        private void CreateDirectories(string currentRunDirectory)
        {
            Directory.CreateDirectory(currentRunDirectory);
            Directory.CreateDirectory(Path.Combine(currentRunDirectory, Constants.TwinsFolder));
            Directory.CreateDirectory(Path.Combine(currentRunDirectory, Constants.RelationshipsFolder));
            if (_settings.IncludeModels)
            {
                Directory.CreateDirectory(Path.Combine(currentRunDirectory, Constants.ModelsFolder));
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using Azure.DigitalTwins.Core;
using System.Collections.Concurrent;
using Willow.AzureDigitalTwins.BackupRestore.Results;
using Willow.AzureDigitalTwins.BackupRestore.Log;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.AzureDigitalTwins.Services.Domain.Instance.Readers;
using Willow.AzureDigitalTwins.BackupRestore.Operations.Base;
using Willow.Model.Requests;
using DTDLParser;

namespace Willow.AzureDigitalTwins.BackupRestore.Operations.Implementations
{
    public class Import : CommandRunner
	{
        private readonly IAzureDigitalTwinWriter _azureDigitalTwinWriter;
		private readonly AzureDigitalTwinReader _azureDigitalTwinReader;
		private readonly Options _settings;
        private readonly ImportResult _processResult;
        private readonly IInteractiveLogger _interactiveLogger;
		private readonly IStorageReader _localFilesReader;
		private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;
		protected SearchOption _searchOption;

		public Import(IAzureDigitalTwinWriter azureDigitalTwinWriter, AzureDigitalTwinReader azureDigitalTwinReader,
			Options settings,
			IInteractiveLogger interactiveLogger,
			IStorageReader localFilesReader,
			IAzureDigitalTwinModelParser azureDigitalTwinModelParser)
        {
			_azureDigitalTwinWriter = azureDigitalTwinWriter;
            _settings = settings;
            _processResult = new ImportResult(settings, interactiveLogger);
            _interactiveLogger = interactiveLogger;
			_localFilesReader = localFilesReader;
			_searchOption = SearchOption.TopDirectoryOnly;
			_azureDigitalTwinReader = azureDigitalTwinReader;
			_azureDigitalTwinModelParser = azureDigitalTwinModelParser;
		}

        public override List<string> GetValidationErrors()
        {
            var errors = new List<string>();
            if (string.IsNullOrEmpty(_settings.ImportSource.Trim()))
            {
                errors.Add("Please provide a valid file or folder using the importsource argument.");
            }

            if (!string.IsNullOrEmpty(_settings.ImportSource.Trim()) && _settings.Zipped && (!File.Exists(_settings.ImportSource) || !Path.GetExtension(_settings.ImportSource).Equals(".zip")))
            {
                errors.Add($"Please make sure you provide a valid zip file. Provided path: {_settings.ImportSource}.");
            }

            if (!string.IsNullOrEmpty(_settings.ImportSource.Trim()) && !_settings.Zipped && !Directory.Exists(_settings.ImportSource))
            {
                errors.Add($"Provide directory not found. Provided path: {_settings.ImportSource}");
            }
            return errors;
        }

		private string GetSourceFolder(string path, bool zipped, string tempPathName)
		{
			if (!zipped)
				return path;

			var sourceFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{tempPathName}.{DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss")}");
			Directory.CreateDirectory(sourceFolder);
			ZipFile.ExtractToDirectory(path, sourceFolder);

			return sourceFolder;
		}

		public override async Task<ProcessResult> ProcessCommand()
        {
			var sourceFolder = GetSourceFolder(_settings.ImportSource, _settings.Zipped, _settings.AdtInstance);

            _interactiveLogger.LogLine($"Import triggered for adt instance {_settings.AdtInstance}.");

            await ProcessModels(sourceFolder);

			if (_processResult.ModelErros.Any())
			{
				_interactiveLogger.LogLine("There has been errors uploading models, please review the output report. You can skip importing models by skipping --includemodels option.");
				return _processResult;
			}

            await ProcessTwins(sourceFolder);
            await ProcessRelationships(sourceFolder);

            CleanUpTemporaryFolder(sourceFolder);

            _interactiveLogger.NewLine();
            _interactiveLogger.LogLine($"Import done for adt instance {_settings.AdtInstance}.");

            return _processResult;
        }

		private void CleanUpTemporaryFolder(string path)
		{
			if (!_settings.Zipped)
				return;

			Directory.Delete(path, true);
		}

		protected virtual List<T> ParseFile<T>(string fileContent)
		{
			return JsonSerializer.Deserialize<List<T>>(fileContent);
		}

		private async Task<int> ProcessData<T>(string sourceFolder, string folderName, string entityType, ConcurrentDictionary<string, string> errors, Func<T, string> getId, Func<T, Task> processEntity, Func<Task> finalStep = null)
        {
            var path = Path.Combine(sourceFolder, folderName);
            if (!Directory.Exists(path))
            {
                _interactiveLogger.LogLine($"No {entityType} to import, no {folderName} folder found in {sourceFolder}.");
                return 0;
            }

            _interactiveLogger.LogLine($"Importing {entityType}...");

			var entities = await _localFilesReader.ReadFiles(path, _searchOption, ParseFile<T>, getId);

            var succeeded = await ProcessWithRetry<T>(entityType, errors, getId, processEntity, _settings, _interactiveLogger, entities, finalStep);
            
            _interactiveLogger.NewLine();
            _interactiveLogger.LogLine($"Done importing {entityType}...");

            return succeeded;
        }

        private async Task ProcessModels(string sourceFolder)
        {
            if (!_settings.IncludeModels)
                return;

            var dtdlModels = new ConcurrentBag<DigitalTwinsModelBasicData>();

            await ProcessData<DigitalTwinsModelBasicData>(sourceFolder,
                Constants.ModelsFolder,
                "models",
                _processResult.ModelErros,
                x => x.Id,
                x => Task.Run(() => dtdlModels.Add(x)),
                () => ImportModels(dtdlModels));
        }

        private async Task ImportModels(IEnumerable<DigitalTwinsModelBasicData> allModels)
        {
            var parser = new ModelParser();
            var modelData = await parser.ParseAsync(allModels.Select(x => x.DtdlModel).ToAsyncEnumerable());
            var existingModelIds = new List<string>();

            _interactiveLogger.NewLine();
            _interactiveLogger.LogLine("Processing and reordering list of models to import.");

			var existingModels = await _azureDigitalTwinReader.GetModelsAsync();
			existingModelIds.AddRange(existingModels.Select(x => x.Id));

            var models = allModels.Where(x => !existingModelIds.Contains(x.Id)).Select(x => new DigitalTwinsModelBasicData { Id = x.Id, DtdlModel = x.DtdlModel }).ToList();

            if (!models.Any())
            {
                _interactiveLogger.LogLine($"No new models to import. Models read from file: {allModels.Count()}, existing models in adt instance: {existingModelIds.Count}.");
                return;
            }

			_interactiveLogger.LogLine($"{models.Count()} models to import...");

			var modelsToSync = models.Select(x => x.Id).ToList();
			IDictionary<DigitalTwinsModelBasicData, IEnumerable<string>> sortedFullModels = null;
			try
			{
				sortedFullModels = await _azureDigitalTwinModelParser.TopologicalSort(models.Union(existingModels.Where(x => models.All(m => m.Id != x.Id))));
			}
			catch (ParsingException parsingException)
			{
				_interactiveLogger.LogLine($"Parsing errors: {string.Join(" | ", parsingException.Errors.Select(x => x.Message))}");
				return;
			}

			foreach (var model in sortedFullModels)
			{
				try
				{
					_processResult.ProcessedModels++;
					_interactiveLogger.Log($"Processing model {_processResult.ProcessedModels}/{models.Count}...", true, false);
					await _azureDigitalTwinWriter.CreateModelsAsync(new List<DigitalTwinsModelBasicData> { model.Key });
				}
				catch (Exception ex)
				{
					_processResult.ModelErros.TryAdd(model.Key.Id, ex.Message);
				}
			}

			_interactiveLogger.NewLine();
        }

        private async Task ProcessTwins(string sourceFolder)
        {
            _processResult.ProcessedTwins = await ProcessData<JsonElement>(sourceFolder,
                Constants.TwinsFolder,
                "twins",
                _processResult.TwinErrors,
                x => JsonSerializer.Deserialize<BasicDigitalTwin>(x.GetRawText()).Id,
                x => ProcessEntity<BasicDigitalTwin>(x, (x, y) => _azureDigitalTwinWriter.CreateOrReplaceDigitalTwinAsync(x)));
        }
                
        private async Task ProcessRelationships(string sourceFolder)
        {
            _processResult.ProcessedRelationships = await ProcessData<JsonElement>(sourceFolder,
                Constants.RelationshipsFolder,
                "relationships",
                _processResult.RelationshipErrors,
                x => JsonSerializer.Deserialize<BasicRelationship>(x.GetRawText()).Id,
                x => ProcessEntity<BasicRelationship>(x, (x, y) =>
				{
					// Remove eTag to avoid If-None-Match error
					x.ETag = null;
					return _azureDigitalTwinWriter.CreateOrReplaceRelationshipAsync(x);
				}));
        }

        private Task ProcessEntity<T>(JsonElement json, Func<T, JsonElement, Task<T>> process)
        {
            var entity = JsonSerializer.Deserialize<T>(json.GetRawText());
            return process(entity, json);
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.DigitalTwins.Core;
using Willow.AzureDigitalTwins.BackupRestore.Log;
using Willow.AzureDigitalTwins.BackupRestore.Operations.Base;
using Willow.AzureDigitalTwins.BackupRestore.Results;
using Willow.AzureDigitalTwins.Services.Domain.Instance.Readers;
using Willow.AzureDigitalTwins.Services.Interfaces;

namespace Willow.AzureDigitalTwins.BackupRestore.Operations.Implementations
{
	public class Clear : CommandRunner
	{
		protected readonly AzureDigitalTwinReader _azureDigitalTwinReader;
		protected readonly Options _settings;
		protected readonly IInteractiveLogger _interactiveLogger;
		protected readonly IAzureDigitalTwinWriter _azureDigitalTwinWriter;
		private readonly ClearResult _processResult;
		private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;

		public Clear(AzureDigitalTwinReader azureDigitalTwinReader,
			Options settings,
			IInteractiveLogger interactiveLogger,
			IAzureDigitalTwinWriter azureDigitalTwinWriter,
			IAzureDigitalTwinModelParser azureDigitalTwinModelParser)
		{
			_azureDigitalTwinReader = azureDigitalTwinReader;
			_settings = settings;
			_interactiveLogger = interactiveLogger;
			_azureDigitalTwinWriter = azureDigitalTwinWriter;
			_processResult = new ClearResult(settings, interactiveLogger);
			_azureDigitalTwinModelParser = azureDigitalTwinModelParser;
		}

		public override List<string> GetValidationErrors()
		{
			return new List<string>();
		}

		public override async Task<ProcessResult> ProcessCommand()
		{
			var deleteRelationships = DeleteRelationships();

			await Task.WhenAll(deleteRelationships);

			var deleteTwins = DeleteTwins();

			await Task.WhenAll(deleteTwins);

			var deleteModels = DeleteModels();

			await Task.WhenAll(deleteModels);

			return _processResult;
		}

		private async Task DeleteTwins()
		{
			_interactiveLogger.LogLine("Retrieving twins...");

			var pageable = _azureDigitalTwinReader.QueryAsync<BasicDigitalTwin>("select $dtId from digitaltwins");
			var pages = await pageable.AsPages().ToListAsync();
			var twins = pages.SelectMany(x => x.Values);

			if (!twins.Any())
			{
				_interactiveLogger.LogLine("No twins found in instance...");
				return;
			}

			_interactiveLogger.LogLine($"{twins.Count()} twins found in instance...");

			_processResult.ProcessedTwins = await ProcessWithRetry<BasicDigitalTwin>("Twins", _processResult.TwinErrors,
				x => x.Id,
				x => _azureDigitalTwinWriter.DeleteDigitalTwinAsync(x.Id),
				_settings, _interactiveLogger,
				new ConcurrentDictionary<string, BasicDigitalTwin>(twins.ToDictionary(x => x.Id, x => x)));
		}

		private async Task DeleteRelationships()
		{
			_interactiveLogger.LogLine("Retrieving relationships...");

			var pageable = _azureDigitalTwinReader.QueryAsync<BasicRelationship>("select r.$relationshipId, r.$sourceId from relationships r");
			var pages = await pageable.AsPages().ToListAsync();
			var relationships = pages.SelectMany(x => x.Values);

			if (!relationships.Any())
			{
				_interactiveLogger.LogLine("No relationships found in instance...");
				return;
			}

			_interactiveLogger.LogLine($"{relationships.Count()} relationships found in instance...");

			_processResult.ProcessedRelationships = await ProcessWithRetry<BasicRelationship>("Relationships", _processResult.RelationshipErrors,
				x => x.Id,
				x => _azureDigitalTwinWriter.DeleteRelationshipAsync(x.SourceId, x.Id),
				_settings, _interactiveLogger,
				new ConcurrentDictionary<string, BasicRelationship>(relationships.ToDictionary(x => x.Id, x => x)));
		}

		private async Task DeleteModels()
		{
			if (_settings.IncludeModels)
			{
				_interactiveLogger.LogLine("Retrieving models...");

				var sourceModels = await _azureDigitalTwinReader.GetModelsAsync();

				if (!sourceModels.Any())
				{
					_interactiveLogger.LogLine("No models found in instance...");
					return;
				}

				var sourceModelCount = sourceModels.Count();
				_interactiveLogger.LogLine($"{sourceModelCount} models found in instance...");

				var sortedModels = await _azureDigitalTwinModelParser.TopologicalSort(sourceModels);
				var modelIdsToDelete = sortedModels.Reverse().Select(x => x.Key).ToList();
				var deletedModels = 0;
				foreach (var model in modelIdsToDelete)
				{
					try
					{
						await _azureDigitalTwinWriter.DeleteModelAsync(model.Id);
						deletedModels++;
						_interactiveLogger.Log($"Deleted {deletedModels}/{sourceModelCount} models...", true, false);
					}
					catch (RequestFailedException e)
					{
						_processResult.ModelErros.TryAdd(model.Id, e.Message);
					}
				}

				_interactiveLogger.Log($"Deleted {deletedModels} models...", true);
				_processResult.ProcessedModels = deletedModels;
			}
		}
	}
}

using System.Threading.Tasks;
using System.Linq;
using Willow.AzureDigitalTwins.BackupRestore.Results;
using System.Collections.Generic;
using Willow.AzureDigitalTwins.BackupRestore.Log;
using Willow.AzureDigitalTwins.Services.Domain.Instance.Readers;
using Willow.AzureDigitalTwins.BackupRestore.Operations.Base;

namespace Willow.AzureDigitalTwins.BackupRestore.Operations.Implementations
{
    public class Stats : CommandRunner
	{
        private readonly AzureDigitalTwinReader _azureDigitalTwinService;
        private readonly Options _settings;
        private readonly StatsResult _statsResult;
        private readonly IInteractiveLogger _interactiveLogger;

        public Stats(AzureDigitalTwinReader azureDigitalTwinService, Options settings, IInteractiveLogger interactiveLogger)
        {
            _azureDigitalTwinService = azureDigitalTwinService;
            _settings = settings;
            _statsResult = new StatsResult(interactiveLogger);
            _interactiveLogger = interactiveLogger;
        }

        public override List<string> GetValidationErrors()
        {
            return new List<string>();
        }

        public override async Task<ProcessResult> ProcessCommand()
        {
            _interactiveLogger.LogLine($"Calculating stats for {_settings.AdtInstance}...");

            _statsResult.TwinsCount = await _azureDigitalTwinService.GetTwinsCountAsync();
            _statsResult.RelationshipsCount = await _azureDigitalTwinService.GetRelationshipsCountAsync();
            var models = await _azureDigitalTwinService.GetModelsAsync();
            _statsResult.ModelsCount = models.Count();

            _interactiveLogger.LogLine("Done calculating stats.");

            return _statsResult;
        }
    }
}

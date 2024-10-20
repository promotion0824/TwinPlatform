using System.Collections.Generic;
using System.Linq;
using Willow.AzureDigitalTwins.BackupRestore.Log;
using Willow.AzureDigitalTwins.BackupRestore.Operations.Implementations;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Willow.AzureDigitalTwins.Services.Domain.Instance.Readers;
using Willow.AzureDigitalTwins.BackupRestore.Operations.Base;

namespace Willow.AzureDigitalTwins.BackupRestore
{
	public interface IProcessorFactory
	{
		CommandRunner GetProcessor(ServiceProvider serviceProvider);
	}

	public class ProcessorFactory : IProcessorFactory
	{
		private readonly IInteractiveLogger _interactiveLogger;
		private readonly Options _settings;

		public ProcessorFactory(Options settings, IInteractiveLogger interactiveLogger)
		{
			_settings = settings;
			_interactiveLogger = interactiveLogger;
		}

		public CommandRunner GetProcessor(ServiceProvider serviceProvider)
		{
			var activeActions = new List<bool> { _settings.Export, _settings.Import, _settings.Stats, _settings.Clear };

			if (activeActions.Count(x => x) > 1)
			{
				_interactiveLogger.SetErrorFormat();
				_interactiveLogger.LogLine("Please provide a single action, export - import - stats - clear can not be combined.");
				_interactiveLogger.ResetFormat();
				return null;
			}

			if (_settings.Export && _settings.Structured)
				return new StructuredExport(serviceProvider.GetRequiredService<AzureDigitalTwinReader>(), _settings, _interactiveLogger);

			if (_settings.Export)
				return new Export(serviceProvider.GetRequiredService<AzureDigitalTwinReader>(), _settings, _interactiveLogger);

			if (_settings.Import && _settings.Structured)
				return new StructuredImport(serviceProvider.GetRequiredService<IAzureDigitalTwinWriter>(), serviceProvider.GetRequiredService<AzureDigitalTwinReader>(), _settings, _interactiveLogger, serviceProvider.GetRequiredService<IStorageReader>(), serviceProvider.GetRequiredService<IAzureDigitalTwinModelParser>());

			if (_settings.Import)
				return new Import(serviceProvider.GetRequiredService<IAzureDigitalTwinWriter>(), serviceProvider.GetRequiredService<AzureDigitalTwinReader>(), _settings, _interactiveLogger, serviceProvider.GetRequiredService<IStorageReader>(), serviceProvider.GetRequiredService<IAzureDigitalTwinModelParser>());

			if (_settings.Stats)
				return new Stats(serviceProvider.GetRequiredService<AzureDigitalTwinReader>(), _settings, _interactiveLogger);

			if (_settings.Clear)
				return new Clear(serviceProvider.GetRequiredService<AzureDigitalTwinReader>(), _settings, _interactiveLogger, serviceProvider.GetRequiredService<IAzureDigitalTwinWriter>(), serviceProvider.GetRequiredService<IAzureDigitalTwinModelParser>());

			_interactiveLogger.SetErrorFormat();
			_interactiveLogger.LogLine("Please indicate a valid action: export, import, clear or stats");
			_interactiveLogger.ResetFormat();

			return null;
		}
	}
}

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Willow.AzureDigitalTwins.BackupRestore.Log;
using Willow.AzureDigitalTwins.Services.Domain.Instance.Readers;
using Willow.AzureDigitalTwins.Services.Interfaces;

namespace Willow.AzureDigitalTwins.BackupRestore.Operations.Implementations
{
	public class StructuredImport : Import
	{
		public StructuredImport(IAzureDigitalTwinWriter azureDigitalTwinWriter, AzureDigitalTwinReader azureDigitalTwinReader, Options settings, IInteractiveLogger interactiveLogger, IStorageReader localFilesReader, IAzureDigitalTwinModelParser azureDigitalTwinModelParser)
			: base (azureDigitalTwinWriter, azureDigitalTwinReader, settings, interactiveLogger, localFilesReader, azureDigitalTwinModelParser)
		{
			_searchOption = SearchOption.AllDirectories;
		}

		protected override List<T> ParseFile<T>(string fileContent)
		{
			return new List<T> { JsonSerializer.Deserialize<T>(fileContent) };
		}
	}
}

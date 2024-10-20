using Willow.AzureDigitalTwins.BackupRestore.Log;
using Azure.DigitalTwins.Core;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using Willow.AzureDigitalTwins.Services.Domain.Instance.Readers;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.BackupRestore.Operations.Implementations
{
    public class StructuredExport : Export
    {
        public StructuredExport(AzureDigitalTwinReader azureDigitalTwinService, Options settings, IInteractiveLogger interactiveLogger) :
			base(azureDigitalTwinService, settings, interactiveLogger)
        {
        }

        private string GetFileSafeName(string name)
        {
            return name.Replace(":", "_");
        }

        protected override async Task ProcessModels(IReadOnlyList<DigitalTwinsModelBasicData> entities, int pageNumber, string currentRunDirectory)
        {
            var process = Task.Run(() => Parallel.ForEach(entities,
                new ParallelOptions { MaxDegreeOfParallelism = 20 },
                async x =>
                {
                    await File.WriteAllTextAsync(Path.Combine(currentRunDirectory, Constants.ModelsFolder, $"{GetFileSafeName(x.Id)}.json"), x.DtdlModel);
                }));

            await process;
        }

        protected override async Task ProcessTwins(IReadOnlyList<BasicDigitalTwin> entities, int pageNumber, string currentRunDirectory)
        {
			var modelFolders = new ConcurrentBag<string>();

            var process = Task.Run(() => Parallel.ForEach(entities,
                new ParallelOptions { MaxDegreeOfParallelism = 20 },
                async x =>
                {
					var folder = Path.Combine(currentRunDirectory, Constants.TwinsFolder, GetFileSafeName(x.Metadata.ModelId));

					if (!modelFolders.Contains(folder))
					{
						Directory.CreateDirectory(folder);
						modelFolders.Add(folder);
					}

                    await File.WriteAllTextAsync(Path.Combine(folder, $"{x.Id}.json"), JsonSerializer.Serialize(x));
                }));

            await process;
        }

        protected override async Task ProcessRelationships(IReadOnlyList<BasicRelationship> entities, int pageNumber, string currentRunDirectory)
        {
            var process = Task.Run(() => Parallel.ForEach(entities,
                   new ParallelOptions { MaxDegreeOfParallelism = 20 },
                   async x =>
                   {
                       await File.WriteAllTextAsync(Path.Combine(currentRunDirectory, Constants.RelationshipsFolder, $"{x.Id}.json"), JsonSerializer.Serialize(x));
                   }));

            await process;
        }
    }
}

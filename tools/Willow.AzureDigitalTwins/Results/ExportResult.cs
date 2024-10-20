using Willow.AzureDigitalTwins.BackupRestore.Log;
using System.IO;

namespace Willow.AzureDigitalTwins.BackupRestore.Results
{
    public class ExportResult : ProcessResult
    {
        private readonly Options _settings;
        public ExportResult(Options settings, IInteractiveLogger interactiveLogger) : base(interactiveLogger)
        {
            _settings = settings;
        }

        public int ExportedTwinsCount { get; set; }
        public int ExportedRelationshipsCount { get; set; }
        public int ExportedModelsCount { get; set; }

        public string OutputDirectory { get; set; }
        public string ZipFile { get; set; }

        public override void GenerateOutput()
        {
            DisplaySummary();

            _interactiveLogger.LogLine($"Export ADT instance {_settings.AdtInstance} done");

            _interactiveLogger.CreateOutputLogFile(OutputDirectory).Wait();

            if (_settings.Zipped)
            {
                _interactiveLogger.LogLine("Creating zip file...");

                System.IO.Compression.ZipFile.CreateFromDirectory(OutputDirectory, ZipFile);
                Directory.Delete(OutputDirectory, true);

                _interactiveLogger.LogLine("Done creating zip file...");
            }

            _interactiveLogger.NewLine();
            _interactiveLogger.SetSuccessFormat();
            _interactiveLogger.LogLine($"Output created in {(_settings.Zipped ? ZipFile : OutputDirectory)}");
            _interactiveLogger.NewLine();
        }

        private void DisplaySummary()
        {
            _interactiveLogger.NewLine();
            _interactiveLogger.SetSuccessFormat();
            _interactiveLogger.LogLine("Exported data summary:");
            _interactiveLogger.LogLine($"Twins: {ExportedTwinsCount}", true);
            _interactiveLogger.LogLine($"Relationships: {ExportedRelationshipsCount}", true);
            if (_settings.IncludeModels)
                _interactiveLogger.LogLine($"Models: {ExportedModelsCount}", true);
            _interactiveLogger.NewLine();

            _interactiveLogger.ResetFormat();
        }
    }
}

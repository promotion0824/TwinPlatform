using Willow.AzureDigitalTwins.BackupRestore.Log;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;

namespace Willow.AzureDigitalTwins.BackupRestore.Results
{
    public class ImportResult : ProcessResult
    {
        protected readonly Options _settings;
        public ImportResult(Options settings, IInteractiveLogger interactiveLogger) : base(interactiveLogger)
        {
            _settings = settings;
            ModelErros = new ConcurrentDictionary<string, string>();
            TwinErrors = new ConcurrentDictionary<string, string>();
            RelationshipErrors = new ConcurrentDictionary<string, string>();
        }

        public int ProcessedTwins { get; set; }
        public int ProcessedRelationships { get; set; }
        public int ProcessedModels { get; set; }
        public ConcurrentDictionary<string, string> ModelErros { get; set; }
        public ConcurrentDictionary<string, string> TwinErrors { get; set; }
        public ConcurrentDictionary<string, string> RelationshipErrors { get; set; }

        public override void GenerateOutput()
        {
            _interactiveLogger.NewLine();

            _interactiveLogger.SetSuccessFormat();
            PrintSummary("Twins", ProcessedTwins, TwinErrors.Count);
            PrintSummary("Relationships", ProcessedRelationships, RelationshipErrors.Count);
            if (_settings.IncludeModels)
                PrintSummary("Models", ProcessedModels, ModelErros.Count);

            _interactiveLogger.ResetFormat();

            if (!ModelErros.Any() && !TwinErrors.Any() && !RelationshipErrors.Any())
                return;

            var errorOutput = CreateErrorFile();

            _interactiveLogger.NewLine();
            _interactiveLogger.SetErrorFormat();
            _interactiveLogger.LogLine($"Errors occurred during import process and have been logged in {Path.GetFullPath(errorOutput)}");

            _interactiveLogger.ResetFormat();
        }

        private string CreateErrorFile()
        {
            var errorOutput = $"erros.{_settings.AdtInstance}.{DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss")}.csv";
            var writer = new StreamWriter(errorOutput, false, Encoding.UTF8);
            writer.WriteLine("Type,Id,Exception");
            AddRow(writer, "Models", ModelErros);
            AddRow(writer, "Twins", TwinErrors);
            AddRow(writer, "Relationships", RelationshipErrors);
            writer.Flush();
            writer.Close();

            return errorOutput;
        }

        protected virtual void PrintSummary(string type, int succeeded, int failed)
        {
            _interactiveLogger.LogLine($"{type}:");
            _interactiveLogger.LogLine($"\tImported {succeeded}", true);
            _interactiveLogger.LogLine($"\tFailed {failed}", true);
        }

        private void AddRow(StreamWriter writer, string type, ConcurrentDictionary<string, string> collection)
        {
            foreach (var error in collection)
            {
                writer.WriteLine($"{type},{Escape(error.Key)},{Escape(error.Value)}");
            }
        }

        private string Escape(string value)
        {
            if (value != null && (value.Contains(',') || value.Contains('\n')))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}

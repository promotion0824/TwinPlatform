using Willow.AzureDigitalTwins.BackupRestore.Log;

namespace Willow.AzureDigitalTwins.BackupRestore.Results
{
    public class StatsResult : ProcessResult
    {
        public int TwinsCount { get; set; }
        public int RelationshipsCount { get; set; }
        public int ModelsCount { get; set; }

        public StatsResult(IInteractiveLogger interactiveLogger) : base(interactiveLogger)
        {
        }

        public override void GenerateOutput()
        {
            _interactiveLogger.SetSuccessFormat();

            _interactiveLogger.LogLine($"Twins: {TwinsCount}");
            _interactiveLogger.LogLine($"Relationships: {RelationshipsCount}");
            _interactiveLogger.LogLine($"Models: {ModelsCount}");

            _interactiveLogger.ResetFormat();
        }
    }
}

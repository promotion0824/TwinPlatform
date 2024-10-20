using Willow.AzureDigitalTwins.BackupRestore.Log;

namespace Willow.AzureDigitalTwins.BackupRestore.Results
{
	public class ClearResult : ImportResult
	{
		public ClearResult(Options settings, IInteractiveLogger interactiveLogger) : base(settings, interactiveLogger)
		{ }

		protected override void PrintSummary(string type, int succeeded, int failed)
		{
			_interactiveLogger.LogLine($"{type}:");
			_interactiveLogger.LogLine($"\tDeleted {succeeded}", true);
			_interactiveLogger.LogLine($"\tFailed {failed}", true);
		}
	}
}

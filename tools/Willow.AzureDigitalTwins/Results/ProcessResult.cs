using Willow.AzureDigitalTwins.BackupRestore.Log;

namespace Willow.AzureDigitalTwins.BackupRestore.Results
{
    public abstract class ProcessResult
    {
        protected readonly IInteractiveLogger _interactiveLogger;

        public ProcessResult(IInteractiveLogger interactiveLogger)
        {
            _interactiveLogger = interactiveLogger;
        }

        public abstract void GenerateOutput();
    }
}

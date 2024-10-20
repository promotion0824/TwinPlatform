using System.Threading.Tasks;

namespace Willow.AzureDigitalTwins.BackupRestore.Log
{
    public interface IInteractiveLogger
    {
        void NewLine();
        void LogLine(string message, bool tab = false);
        void Log(string message, bool replaceCurrentLine = false, bool includeInOutputFile = true);
        void SetSuccessFormat();
        void SetErrorFormat();
        void ResetFormat();
        Task CreateOutputLogFile(string folder);
    }
}

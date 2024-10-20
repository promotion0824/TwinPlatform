#pragma warning disable CA2254 // Template should be a static expression
namespace ConnectorCore.Database
{
    using global::DbUp.Engine.Output;
    using Microsoft.Extensions.Logging;

    internal class DbUpgradeLog(ILogger logger) : IUpgradeLog
    {
        public void WriteInformation(string format, params object[] args)
        {
            logger.LogInformation(format, args);
        }

        public void WriteError(string format, params object[] args)
        {
            logger.LogError(format, args);
        }

        public void WriteWarning(string format, params object[] args)
        {
            logger.LogWarning(format, args);
        }
    }
}
#pragma warning restore CA2254 // Template should be a static expression

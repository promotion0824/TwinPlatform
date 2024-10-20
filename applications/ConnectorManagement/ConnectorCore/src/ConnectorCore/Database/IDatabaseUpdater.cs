namespace ConnectorCore.Database
{
    using Microsoft.Extensions.Logging;

    internal interface IDatabaseUpdater
    {
        void DeployDatabaseChanges(ILoggerFactory loggerFactory, bool isDevEnvironment);
    }
}

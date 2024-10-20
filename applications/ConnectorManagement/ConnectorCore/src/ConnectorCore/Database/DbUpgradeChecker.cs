namespace ConnectorCore.Database
{
    using System;
    using System.Reflection;
    using ConnectorCore.Database.DbUp;
    using global::DbUp;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    internal class DbUpgradeChecker : IDbUpgradeChecker
    {
        private readonly IConfiguration configuration;
        private readonly ILoggerFactory loggerFactory;

        public DbUpgradeChecker(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.loggerFactory = loggerFactory;
        }

        public void EnsureDatabaseUpToDate(IWebHostEnvironment env)
        {
            var dbUpgradeLogger = loggerFactory.CreateLogger<DbUpgradeChecker>();
            var connectionString = configuration.GetConnectionString(Constants.DatabaseName);

            try
            {
                var logger = new DbUpgradeLog(dbUpgradeLogger);
                EnsureDatabase.For.AzureSqlDatabase(connectionString, logger);

                var upgradeEngine = DeployChanges.To.AzureSqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .WithTransactionPerScript()
                    .LogScriptOutput()
                    .LogTo(logger)
                    .LogToConsole()
                    .Build();

                if (!upgradeEngine.IsUpgradeRequired())
                {
                    dbUpgradeLogger.LogInformation("Database upgrade is not required.");
                    return;
                }

                if (!upgradeEngine.PerformUpgrade().Successful)
                {
                    throw new Exception("DbUp upgrade engine failed to perform upgrade.");
                }
            }
            catch (Exception ex)
            {
                dbUpgradeLogger.LogCritical(ex, "Failed to create or upgrade database.");
                throw;
            }
        }
    }
}

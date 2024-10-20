using System;
using DbUp;
using DirectoryCore.Database.DbUp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DirectoryCore.Database
{
    public interface IDbUpgradeChecker
    {
        void EnsureDatabaseUpToDate(IWebHostEnvironment env);
    }

    public class DbUpgradeChecker : IDbUpgradeChecker
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public DbUpgradeChecker(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public void EnsureDatabaseUpToDate(IWebHostEnvironment env)
        {
            var dbUpgradeLogger = _loggerFactory.CreateLogger<DbUpgradeChecker>();
            var connectionString = _configuration.GetConnectionString(Constants.DatabaseName);

            try
            {
                var logger = new DbUpgradeLog(dbUpgradeLogger);
                EnsureDatabase.For.AzureSqlDatabase(connectionString, logger, -1, null);

                var upgradeEngine = DeployChanges
                    .To.AzureSqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(typeof(DbUpgradeChecker).Assembly)
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
                    throw new InvalidOperationException(
                        "DbUp upgrade engine failed to perform upgrade."
                    );
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

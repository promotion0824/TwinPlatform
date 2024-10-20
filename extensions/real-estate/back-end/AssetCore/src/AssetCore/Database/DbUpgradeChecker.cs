using System;
using System.Reflection;
using AssetCore.Database.DbUp;
using DbUp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AssetCore.Database
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
            var connectionString = _configuration.GetConnectionString("MappingDb");

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

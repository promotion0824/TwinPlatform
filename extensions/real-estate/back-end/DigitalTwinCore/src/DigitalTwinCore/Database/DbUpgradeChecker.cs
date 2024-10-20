using DbUp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DigitalTwinCore.Database.DbUp;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DigitalTwinCore.Database
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
            var connectionString = _configuration.GetConnectionString("DigitalTwinDb");

            try
            {
                var logger = new DbUpgradeLog(dbUpgradeLogger);
                EnsureDatabase.For.AzureSqlDatabase(connectionString, logger);

                var upgradeEngine = DeployChanges.To.AzureSqlDatabase(connectionString)
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
                    throw new ApplicationException("DbUp upgrade engine failed to perform upgrade.");
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

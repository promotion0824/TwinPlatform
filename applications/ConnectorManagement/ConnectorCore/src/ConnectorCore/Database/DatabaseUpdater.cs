namespace ConnectorCore.Database
{
    using System.Reflection;
    using ConnectorCore.Database.DbUp;
    using ConnectorCore.Infrastructure.HealthCheck;
    using global::DbUp;

    internal class DatabaseUpdater : IDatabaseUpdater
    {
        private readonly IDbConnectionStringProvider connectionStringProvider;
        private readonly HealthCheckSql healthCheckSql;

        public DatabaseUpdater(IDbConnectionStringProvider connectionStringProvider, HealthCheckSql healthCheckSql)
        {
            this.connectionStringProvider = connectionStringProvider;
            this.healthCheckSql = healthCheckSql;
        }

        public virtual void DeployDatabaseChanges(ILoggerFactory loggerFactory, bool isDevEnvironment)
        {
            var dbUpgradeLogger = loggerFactory.CreateLogger<DatabaseUpdater>();
            var connectionString = connectionStringProvider.GetConnectionString();

            try
            {
                var logger = new DbUpgradeLog(dbUpgradeLogger);
                healthCheckSql.Current = HealthCheckSql.Healthy;
                EnsureDatabase.For.AzureSqlDatabase(connectionString, logger);

                var upgradeEngine = DeployChanges.To.AzureSqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .WithTransactionPerScript()
                    .LogToAutodetectedLog()
                    .LogScriptOutput()
                    .LogToConsole()
                    .Build();

                if (!upgradeEngine.IsUpgradeRequired())
                {
                    dbUpgradeLogger.LogInformation("Database upgrade is not required.");
                    return;
                }

                var upgradeDb = upgradeEngine.PerformUpgrade();
                if (upgradeDb.Successful == false)
                {
                    if (isDevEnvironment)
                    {
                        var dbUpException = new Exception($"[DEV]DbUp upgrade engine failed to perform upgrade. Script names: {string.Join(",", upgradeDb.Scripts.Select(s => s.Name))}.", upgradeDb.Error);
                        throw dbUpException;
                    }
                    else
                    {
                        throw new Exception("DbUp upgrade engine failed to perform upgrade.");
                    }
                }
            }
            catch (Exception ex)
            {
                healthCheckSql.Current = HealthCheckSql.ConnectionFailed;
                dbUpgradeLogger.LogCritical(ex, "Failed to create or upgrade database.");
                throw;
            }
        }
    }
}

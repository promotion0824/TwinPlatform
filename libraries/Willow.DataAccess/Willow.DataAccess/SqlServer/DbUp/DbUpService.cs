namespace Willow.DataAccess.SqlServer;

using System.Reflection;
using Dapper;
using global::DbUp;
using Willow.DataAccess.SqlServer.DbUp;
using Willow.DataAccess.SqlServer.DbUp.TypeHandlers;

/// <summary>
/// A service for running DbUp migrations.
/// </summary>
public class DbUpService : IDbUpService
{
    private readonly string connectionString;
    private bool hasCompleted;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbUpService"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string to the database.</param>
    public DbUpService(string connectionString)
    {
        this.connectionString = connectionString;
    }

    /// <inheritdoc/>
    public void SetupDapper()
    {
        SqlMapper.AddTypeHandler(new DateTimeHandler());
    }

    /// <inheritdoc/>
    public void AddEnumHandler<T>()
    {
        SqlMapper.AddTypeHandler(typeof(T), new EnumHandler());
    }

    /// <inheritdoc/>
    public void EnsureDatabaseUpToDate(Assembly assembly)
    {
        EnsureDatabase.For.AzureSqlDatabase(connectionString);

        var upgrader = DeployChanges.To.AzureSqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(assembly)
            .LogToConsole()
            .WithExecutionTimeout(TimeSpan.FromSeconds(180))
            .Build();

        if (upgrader.IsUpgradeRequired())
        {
            var result = upgrader.PerformUpgrade();
            if (!result.Successful)
            {
                throw new InvalidOperationException($"DbUp failed to execute migration. {result.Error}");
            }

            hasCompleted = true;
        }
    }

    /// <inheritdoc/>
    public bool HasCompleted()
    {
        return hasCompleted;
    }
}

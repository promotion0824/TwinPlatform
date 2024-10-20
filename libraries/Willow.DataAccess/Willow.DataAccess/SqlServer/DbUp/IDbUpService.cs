namespace Willow.DataAccess.SqlServer;

using System.Reflection;

/// <summary>
/// A service for running DbUp migrations.
/// </summary>
public interface IDbUpService
{
    /// <summary>
    /// Whether or not the database has completed the initial setup.
    /// </summary>
    /// <returns>True if the operation has completed. False otherwise.</returns>
    bool HasCompleted();

    /// <summary>
    /// Ensures the database is up to date.
    /// </summary>
    /// <param name="assembly">The assembly containing the updates.</param>
    void EnsureDatabaseUpToDate(Assembly assembly);

    /// <summary>
    /// Initializes Dapper.
    /// </summary>
    void SetupDapper();

    /// <summary>
    /// Adds an enum handler to Dapper.
    /// </summary>
    /// <typeparam name="T">The enum type to add.</typeparam>
    void AddEnumHandler<T>();
}

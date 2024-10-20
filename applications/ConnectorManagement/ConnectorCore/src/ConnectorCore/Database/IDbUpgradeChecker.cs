namespace ConnectorCore.Database;

internal interface IDbUpgradeChecker
{
    void EnsureDatabaseUpToDate(IWebHostEnvironment env);
}

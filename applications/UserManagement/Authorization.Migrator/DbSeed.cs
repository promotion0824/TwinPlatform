using System;
using System.Linq;
using System.Threading.Tasks;
using Authorization.TwinPlatform.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Authorization.Migrator;

internal static class DbSeed
{
    public static async Task Initialize(TwinPlatformAuthContext authContext)
    {
        var pendingMigrations = await authContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await authContext.Database.MigrateAsync();
        }
    }


    public static async Task SqlDeleteStaleApplications(TwinPlatformAuthContext authContext, ILogger logger)
    {
        try
        {
            // Delete stale applications
            var deleteStaleAppSql = "Delete from Applications where Name in ('WillowTwinApp')";
            await authContext.Database.ExecuteSqlRawAsync(deleteStaleAppSql);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running authorization ad-hoc script.");
        }
    }
}

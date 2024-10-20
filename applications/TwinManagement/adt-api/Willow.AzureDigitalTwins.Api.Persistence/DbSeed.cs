using Microsoft.EntityFrameworkCore;

namespace Willow.AzureDigitalTwins.Api.Persistence;

public static class DbSeed
{
    public static async Task Initialize(DbContext mappingContext)
    {
        var pendingMigrations = await mappingContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await mappingContext.Database.MigrateAsync();
        }
    }
}

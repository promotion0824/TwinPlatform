using System;
using Microsoft.EntityFrameworkCore;
using Willow.IoTService.Deployment.DataAccess.Db;

namespace Willow.IoTService.Deployment.DataAccess.IntegrationTests;

public static class DbContextHelper
{
    public static DeploymentDbContext GetContextWithFreshDb(string dbName, BaseEntitySaveChangesInterceptor? interceptor = null)
    {
        var options = new DbContextOptionsBuilder<DeploymentDbContext>().UseSqlServer($"Server=localhost,1439;Initial Catalog={dbName};Integrated Security=false;User Id=sa;Password=MyPass@word;Encrypt=False;")
                                                                        .LogTo(Console.WriteLine)
                                                                        .EnableDetailedErrors()
                                                                        .EnableSensitiveDataLogging()
                                                                        .Options;

        var context = new DeploymentDbContext(options, interceptor);
        context.Database.EnsureDeleted();
        context.Database.Migrate();
        context.SaveChanges();

        return context;
    }

    public static DeploymentDbContext GetContextWithExistingDb(string dbName,  BaseEntitySaveChangesInterceptor? interceptor = null)
    {
        var options = new DbContextOptionsBuilder<DeploymentDbContext>().UseSqlServer($"Server=localhost,1439;Initial Catalog={dbName};Integrated Security=false;User Id=sa;Password=MyPass@word;Encrypt=False;")
                                                                        .LogTo(Console.WriteLine)
                                                                        .EnableDetailedErrors()
                                                                        .EnableSensitiveDataLogging()
                                                                        .Options;

        var context = new DeploymentDbContext(options, interceptor);
        return context;
    }
    
    public static void DropTestDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<DeploymentDbContext>().UseSqlServer($"Server=localhost,1439;Initial Catalog={dbName};Integrated Security=false;User Id=sa;Password=MyPass@word;Encrypt=False;")
                                                                        .LogTo(Console.WriteLine)
                                                                        .EnableDetailedErrors()
                                                                        .EnableSensitiveDataLogging()
                                                                        .Options;

        using var context = new DeploymentDbContext(options);
        context.Database.EnsureDeleted();
    }
}
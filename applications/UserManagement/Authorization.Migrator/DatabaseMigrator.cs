using System;
using System.IO;
using System.Threading.Tasks;
using Authorization.TwinPlatform.Persistence.Contexts;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Willow.DataAccess.SqlServer;

namespace Authorization.Migrator;

public class DatabaseMigrator : IDesignTimeDbContextFactory<TwinPlatformAuthContext>
{
	private static ILogger<DatabaseMigrator> logger = LoggerFactory.Create(config =>
														{
															config.AddConsole();
														}).CreateLogger<DatabaseMigrator>();

	public static async Task Main()
	{
		await MigrateAsync();
	}

	public TwinPlatformAuthContext CreateDbContext(string[] args)
	{
		var configuration = GetConfiguration();
		return CreateDbContext(configuration);
	}

	private static TwinPlatformAuthContext CreateDbContext(IConfiguration configuration)
	{
		logger.LogTrace("Creating Twin Platform DB Context");
		Microsoft.Data.SqlClient.SqlAuthenticationProvider.SetProvider(SqlAuthenticationMethod.ActiveDirectoryManagedIdentity,
			new AzureSqlAuthProvider());

		try
		{
			var connectionString = configuration.GetAuthorizationDbConnectionString();

            var optionsBuilder = new DbContextOptionsBuilder<TwinPlatformAuthContext>()
                .UseSqlServer(connectionString,
                    opts =>
                    {
                        opts.MigrationsAssembly("Authorization.Migrator");
                        opts.EnableRetryOnFailure();
                    })
                .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));

			return new TwinPlatformAuthContext(optionsBuilder.Options);
		}
		catch (Exception ex)
		{
			logger.LogError(ex,"Error while Creating Twin Platform DB Context");
			throw;
		}
	}

	private static IConfiguration GetConfiguration()
	{
		logger.LogTrace("Getting TwinPlatform Database Connection string");

		var configurationBuilder = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.migrator.json", optional: false)
			.AddEnvironmentVariables();

		return configurationBuilder.Build();
	}

    /// <summary>
    /// Migrate database to the current schema. Get the migration from the current assembly.
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <returns>True if success; false if not.</returns>
	public static async Task<bool> MigrateAsync(IConfiguration? configuration=null)
	{
		try
		{
			configuration ??= GetConfiguration();

			var runMigrations = configuration.GetValue<bool>("RunMigrations");
			logger.LogInformation("Application is set to Run Database Migration: {RunMigrations}", runMigrations);
			if (runMigrations)
			{
				await using var context = CreateDbContext(configuration);
				logger.LogTrace("Initializing Authorization Database migration.");
				await DbSeed.Initialize(context);
                await DbSeed.SqlDeleteStaleApplications(context, logger);
				logger.LogTrace("Authorization Database migration completed.");
			}
            return true;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Authorization Database migration failed.");
            return false;
		}
	}
}

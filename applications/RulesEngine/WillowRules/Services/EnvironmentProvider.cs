using System;
using System.Linq;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Willow.Rules.Configuration;
using Willow.Rules.Logging;
using Willow.Rules.Repository;
using Willow.Rules.Sources;
using WillowRules.Extensions;

namespace Willow.Rules.Services;

/// <summary>
/// Provides everything necessary for a specific customer / environment deployment
/// </summary>
public interface IEnvironmentProvider
{
	/// <summary>
	/// Creates the willow environment passing it all the configured repositories and services
	/// </summary>
	WillowEnvironment Create();
}

/// <summary>
/// Provides everything necessary for a specific customer / environment deployment
/// </summary>
public class EnvironmentProvider : IEnvironmentProvider
{
	private readonly CustomerOptions customerOptions;
	private readonly ILogger<EnvironmentProvider> logger;

	/// <summary>
	/// Creates a new <see cref="EnvironmentProvider"/>
	/// </summary>
	public EnvironmentProvider(
		IOptions<CustomerOptions> customerOptions,
		ILogger<EnvironmentProvider> logger)
	{
		this.customerOptions = customerOptions?.Value ?? throw new ArgumentNullException(nameof(customerOptions));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public WillowEnvironment Create()
	{
		if (string.IsNullOrEmpty(customerOptions.Id)) throw new ArgumentNullException("Customer options Id empty");

		if (customerOptions.SQL is null)
		{
			logger.LogWarning($"SQL is null for {JsonConvert.SerializeObject(customerOptions)}");
			throw new ArgumentException($"SQL is null on environment {customerOptions.Id}");
		}

		logger.LogInformation("Creating Willow Environment and initializing database {sql}", customerOptions.SQL.ConnectionString);

		string sqlConnection = customerOptions.SQL.ConnectionString;

		//logger.LogInformation("Creating Willow Environment {id}", customerEnvironment.Id);

		if (string.IsNullOrEmpty(sqlConnection))
		{
			throw new ArgumentException(nameof(sqlConnection));
		}

		var dbContextOptions = new DbContextOptionsBuilder<RulesContext>()
			.UseSqlServer(sqlConnection, b => b.MigrationsAssembly("WillowRules"))
			.EnableSensitiveDataLogging()
			.Options;

		try
		{
			OnceOnly<string>.Execute("dbinitialize", () =>
			{
				using (var context = new RulesContext(dbContextOptions))
				{
					int count = context.Database.GetMigrations().Count();
					using (var timed = logger.TimeOperation("Migrate database using {count} migrations", count))
					{
						DbInitializer.Initialize(context, logger);
					}
				}
				return "Migrated";
			});
		}
		catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.StartsWith("Login failed"))
		{
			// A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible.
			logger.LogError("Failed to login to database, check DNS name and permissions");
			logger.LogInformation(sqlConnection);
			// Login failed, this is non-recoverable and spamming the log over and over doesn't help
			System.Threading.Thread.Sleep(60000);
			throw;
		}
		catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.StartsWith("DefaultAzureCredential failed to retrieve a token"))
		{
			logger.LogError("Default Azure Credential failed, check ManagedIdentity configuration");
			logger.LogInformation(sqlConnection);
			// Login failed, this is non-recoverable and spamming the log over and over doesn't help
			System.Threading.Thread.Sleep(10000);
			Environment.Exit(1);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to migrate database");
			// And since we cannot do anything else, tell Kubernetes to restart us
			System.Threading.Thread.Sleep(10000);
			Environment.Exit(1);
			//throw;
		}

		logger.LogInformation("Created Willow Environment");

		var we = new WillowEnvironment(customerOptions);

		return we;
	}
}

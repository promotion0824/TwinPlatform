using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Willow.Rules;
using Willow.Rules.Configuration;
using Willow.Rules.Logging;
using Willow.Rules.Repository;
using Willow.Rules.Sources;
using Willow.ServiceBus;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Init container startup code for Rules Engine
/// </summary>
public class Initializer : BackgroundService
{
	private readonly ILogger<Initializer> logger;
	private readonly IOptions<CustomerOptions> customerOptions;

	/// <summary>
	/// Creates a new <see cref="Initializer" />
	/// </summary>
	public Initializer(ILogger<Initializer> logger, IOptions<CustomerOptions> customerOptions)
	{
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.customerOptions = customerOptions ?? throw new ArgumentNullException(nameof(customerOptions));
	}

	/// <summary>
	/// Initializer starting
	/// </summary>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Database initializer starting");

		string sqlConnection = customerOptions.Value.SQL.ConnectionString;

		var dbContextOptions = new DbContextOptionsBuilder<RulesContext>()
			.UseSqlServer(sqlConnection, b => b.MigrationsAssembly("WillowRules"))
			.EnableSensitiveDataLogging()
			.Options;

		try
		{
			using (var context = new RulesContext(dbContextOptions))
			{
				int count = context.Database.GetMigrations().Count();
				using (var timed = logger.TimeOperation("Migrate database using {count} migrations", count))
				{
					DbInitializer.Initialize(context, logger);
				}
			}
			logger.LogInformation("Database initializer completed");

			// And close the whole app nicely
			Environment.Exit(0);
		}
		catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.StartsWith("Login failed"))
		{
			// A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible.
			logger.LogError("Failed to login to database, check DNS name and permissions");
			// Login failed, this is non-recoverable and spamming the log over and over doesn't help
		}
		catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.StartsWith("DefaultAzureCredential failed to retrieve a token"))
		{
			logger.LogError("Default Azure Credential failed, check ManagedIdentity configuration");
			// Login failed, this is non-recoverable and spamming the log over and over doesn't help
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to migrate database");
			// And since we cannot do anything else, tell Kubernetes to restart us
		}

		// And close the whole app badly
		Environment.Exit(1);
	}
}
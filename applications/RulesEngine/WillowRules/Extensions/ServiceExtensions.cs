using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Willow.Rules.Cache;
using Willow.Rules.Configuration;
using Willow.Rules.Repository;
using Willow.Rules.Sources;

namespace WillowRules.Extensions;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/>
/// </summary>
public static partial class ServiceExtensions
{
	/// <summary>
	/// Adds Sql Server as a Distributed Cache
	/// </summary>
	/// <returns></returns>
	public static IServiceCollection AddSqlServerDistributedCache(this IServiceCollection services)
	{
		//Sql Server Cache has its own extension, but we require transient to resolve current customer connection string
		services.AddTransient<IRulesDistributedCache>(s =>
		{
			var options = s.GetRequiredService<IOptions<CustomerOptions>>().Value;
			var env = s.GetRequiredService<WillowEnvironment>();
			var logger = s.GetRequiredService<ILogger<RulesSqlServerCache>>();
			return new RulesSqlServerCache(options.SQL.ConnectionString, logger);
		});

		return services;
	}

	public static IServiceCollection AddRulesDBContext(this IServiceCollection services)
	{
		// Transient, new RulesContext per usage
		services.AddTransient<RulesContext, RulesContext>((sp) =>
		{
			var environment = sp.GetRequiredService<IHostEnvironment>();
			var customerOptions = sp.GetRequiredService<IOptions<CustomerOptions>>();
			string sqlConnection = customerOptions.Value.SQL.ConnectionString;
			var dbContextOptions = new DbContextOptionsBuilder<RulesContext>()
				.UseSqlServer(sqlConnection, b =>
					b.MigrationsAssembly("WillowRules")
					//Disable EF Retry strategy. It forces all data into memory for linq queries
					//https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying which 
					//.ExecutionStrategy(es => new AzureSqlExecutionStrategy(
					// es,
					// maxRetryCount: 5,
					// maxRetryDelay: TimeSpan.FromSeconds(1),
					// errorNumbersToAdd: null,
					// logger: sp.GetRequiredService<ILogger<AzureSqlExecutionStrategy>>()))
					//.UseAzureSqlDefaults(!environment.IsDevelopment())
					)
				.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
				.EnableSensitiveDataLogging()
				.Options;

			return new RulesContext(dbContextOptions);
		});

		services.AddDbContextFactory<RulesContext>((sp, b) =>
		{
			var environment = sp.GetRequiredService<IHostEnvironment>();
			var customerOptions = sp.GetRequiredService<IOptions<CustomerOptions>>();
			string sqlConnection = customerOptions.Value.SQL.ConnectionString;
			b.UseSqlServer(sqlConnection, a => a
				.MigrationsAssembly("WillowRules")
				 //Disable EF Retry strategy. It forces all data into memory for linq queries
				 //https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying which 
				 //.ExecutionStrategy(es => new AzureSqlExecutionStrategy(
				 //	 es,
				 //	 maxRetryCount: 5,
				 //	 maxRetryDelay: TimeSpan.FromSeconds(1),
				 //	 errorNumbersToAdd: null,
				 //	 logger: sp.GetRequiredService<ILogger<AzureSqlExecutionStrategy>>()))
				 //.UseAzureSqlDefaults(!environment.IsDevelopment())
				 )
			.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
			.EnableSensitiveDataLogging();
		});

		return services;
	}
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RulesEngine.Processor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.CognitiveSearch;
using Willow.Extensions.Logging;
using Willow.Rules.Search;
using Willow.Rules.Services;

namespace Willow.Rules.Processor;

/// <summary>
/// Startup health checks to external resources
/// </summary>
public class StartupHealthCheckService : BackgroundService
{
	private readonly IADXService adxService;
	private readonly IRulesSearchBuilderService searchBuilderService;
	private readonly IADTService adtService;
	private readonly IADTApiService adtApiService;
	private readonly HealthCheckADT healthCheckADT;
	private readonly HealthCheckSearch healthCheckSearch;
	private readonly HealthCheckADTApi healthCheckADTApi;
	private readonly ICommandInsightService commandInsightService;
	private readonly ICommandService commandService;
	private readonly ILogger logger;

	/// <summary>
	/// Constructor
	/// </summary>
	public StartupHealthCheckService(
		IADXService adxService,
		IRulesSearchBuilderService searchBuilderService,
		IADTService adtService,
		ICommandInsightService commandInsightService,
		IADTApiService adtApiService,
		ICommandService commandService,
		HealthCheckADT healthCheckADT,
		HealthCheckSearch healthCheckSearch,
		HealthCheckADTApi healthCheckADTApi,
		ILogger<StartupHealthCheckService> logger)
	{
		this.adxService = adxService ?? throw new ArgumentNullException(nameof(adxService));
		this.searchBuilderService = searchBuilderService ?? throw new ArgumentNullException(nameof(searchBuilderService));
		this.adtService = adtService ?? throw new ArgumentNullException(nameof(adtService));
		this.adtApiService = adtApiService ?? throw new ArgumentNullException(nameof(adtApiService));
		this.healthCheckADT = healthCheckADT ?? throw new ArgumentNullException(nameof(healthCheckADT));
		this.healthCheckSearch = healthCheckSearch ?? throw new ArgumentNullException(nameof(healthCheckSearch));
		this.healthCheckADTApi = healthCheckADTApi ?? throw new ArgumentNullException(nameof(healthCheckADTApi));
		this.commandInsightService = commandInsightService ?? throw new ArgumentNullException(nameof(commandInsightService));
		this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Check health
	/// </summary>
	/// <param name="stoppingToken"></param>
	/// <returns></returns>
	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// See https://github.com/dotnet/runtime/issues/36063#issuecomment-518913079
		return Task.Run(async () =>
		{
			await Task.Yield();

			logger.LogInformation("Health Check service starting");

			TimeSpan delay = TimeSpan.FromSeconds(30);

			int totalRetries = 0;
			int maxRetries = 10;

			using (var timed = logger.TimeOperation("Check Health"))

				//Retry until success or max retries have been reached
				while (!stoppingToken.IsCancellationRequested)
				{
					var success = await CheckHealthStatus();

					if (success || totalRetries > maxRetries)
					{
						break;
					}

					logger.LogInformation("Waiting {delay} before next health check. Total Retries {retries}/{maxRetries}", delay, totalRetries, maxRetries);

					//wait a while
					await Task.Delay(delay, stoppingToken);

					totalRetries++;
				}
		});
	}

	private async Task<bool> CheckHealthStatus()
	{
		bool success = true;
		try
		{
			var token = new CancellationTokenSource(TimeSpan.FromSeconds(100));
			await adxService.RunRawQuery(DateTime.Now.AddSeconds(-1), DateTime.Now, cancellationToken: token.Token).ToListAsync();
		}
		catch (Exception ex)
		{
			success = false;
			logger.LogError(ex, "ADX Startup health checks failed");
		}

		try
		{
			var token = new CancellationTokenSource(TimeSpan.FromSeconds(100));
			//index doesn't have to exist, but we shouldn't get connection/auth exceptions
			healthCheckSearch.Current = await searchBuilderService.CheckHealth(cancellationToken: token.Token);
		}
		catch (Exception ex)
		{
			success = false;
			logger.LogError(ex, "Search Service Startup health checks failed");
			healthCheckSearch.Current = HealthCheckSearch.FailingCalls;
		}

		try
		{
			await commandInsightService.TryAcquireToken();
		}
		catch (Exception ex)
		{
			success = false;
			logger.LogError(ex, "Public Api Startup health checks failed");
		}

		try
		{
			var token = new CancellationTokenSource(TimeSpan.FromSeconds(100));
			await commandService.TryAcquireToken();
		}
		catch (Exception ex)
		{
			success = false;
			logger.LogError(ex, "Command Api Startup health checks failed");
		}

		try
		{
			var token = new CancellationTokenSource(TimeSpan.FromSeconds(100));
			var twins = await adtService.AdtInstances[0].ADTClient.GetTwinsCountAsync();
			var relationships = await adtService.AdtInstances[0].ADTClient.GetRelationshipsCountAsync();
			healthCheckADT.Current = HealthCheckADT.HealthyWithCounts(twins, relationships);
		}
		catch (Exception ex)
		{
			success = false;
			healthCheckADT.Current = HealthCheckADT.ConnectionFailed;
			logger.LogError(ex, "ADT Startup health checks failed");
		}

		try
		{
			await adtApiService.TryGetTwinsCount();
		}
		catch (Exception ex)
		{
			success = false;
			logger.LogError(ex, "ADT Api Service Startup health checks failed");
		}

		return success;
	}
}

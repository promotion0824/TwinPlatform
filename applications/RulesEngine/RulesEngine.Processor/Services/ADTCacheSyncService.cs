using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Willow.Extensions.Logging;
using Willow.Rules;
using Willow.Rules.Configuration;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;

namespace RulesEngine.Processor.Services;

/// <summary>
/// Background service that runs adt cache syncs at scheduled intervals.
/// </summary>
public class ADTCacheSyncService : BackgroundService
{
	private readonly ILogger<ADTCacheSyncService> logger;
	private readonly IADXService adxService;
	private readonly IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest;
	private readonly IRepositoryADTSummary repositoryADTSummary;
	private readonly WillowEnvironment willowEnvironment;
	private string timeZone;

	/// <summary>
	/// Creates a new <see cref="ADTCacheSyncService"/>.
	/// </summary>
	public ADTCacheSyncService(
		IADXService adxService,
		IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest,
		IRepositoryADTSummary repositoryADTSummary,
		WillowEnvironment willowEnvironment,
		ILogger<ADTCacheSyncService> logger)
	{
		this.adxService = adxService ?? throw new ArgumentNullException(nameof(adxService));
		this.repositoryRuleExecutionRequest = repositoryRuleExecutionRequest ?? throw new ArgumentNullException(nameof(repositoryRuleExecutionRequest));
		this.repositoryADTSummary = repositoryADTSummary ?? throw new ArgumentNullException(nameof(repositoryADTSummary));
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Main loop for background service.
	/// </summary>
	protected override Task ExecuteAsync(CancellationToken cancellationToken)
	{
		// See https://github.com/dotnet/runtime/issues/36063#issuecomment-518913079
		return Task.Run(async () =>
		{
			await Task.Yield();

			var throttledErrorLogger = logger.Throttle(TimeSpan.FromMinutes(30));

			using var scope = logger.BeginRequestScope(Progress.CacheId, Guid.NewGuid().ToString(), RulesOptions.ProcessorCloudRoleName);

			await Task.Delay(TimeSpan.FromMinutes(2));  // give migrations etc. a chance

			logger.LogInformation("ADT sync execution starting");

			//on startup, get everything from today (0AM)
			DateTime startTime = DateTime.Today;

			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					var timeZoneInfo = await GetTimezone();

					var utcNow = DateTime.UtcNow;

					var tzNow = utcNow.ConvertToDateTimeOffset(timeZoneInfo);

					var tomorrow = tzNow.AddDays(1);

					tomorrow = tomorrow - tomorrow.TimeOfDay;

					//wait until 2AM the next day
					TimeSpan timespan = (tomorrow - tzNow) + TimeSpan.FromHours(2);

					logger.LogInformation("Starting ADT Cache sync in {timespan} at {time}(UTC), {tztime}({tz})", timespan, utcNow + timespan, tzNow + timespan, timeZoneInfo.Id);

					await Task.Delay(timespan, cancellationToken);

					try
					{
						DateTime startTimeUTC = startTime.ToUniversalTime();
						startTime = DateTime.Now;
						await CreateSyncRequests(startTimeUTC, cancellationToken);
					}
					catch (OperationCanceledException)
					{
						throw;
					}
					catch (Exception e)
					{
						throttledErrorLogger.LogError(e, "Adt sync execution failure");
					}
				}

				logger.LogInformation("Adt sync execution complete");
			}
			catch (OperationCanceledException)
			{
				// ignore it, happens when cancellation token fires on shutdown
			}
			catch (Exception e)
			{
				logger.LogError(e, "Adt sync execution failed to run");
			}
		});
	}

	private async Task CreateSyncRequests(DateTime startTimeUTC, CancellationToken cancellationToken)
	{
		var changes = await adxService.HasADTChanges(startTimeUTC);

		logger.LogInformation("ADT sync check since {date}: twin changes {twinchanges}, rel changes {relchanges}", startTimeUTC, changes.hasTwinChanges, changes.hasRelationshipChanges);

		//force at least one cache in an env with nothing
		var hasCacheRequests = await repositoryRuleExecutionRequest.Any(v => v.ProgressId == Progress.CacheId);

		if (changes.hasTwinChanges || changes.hasRelationshipChanges || !hasCacheRequests)
		{
			//1. first run cache update
			var request = RuleExecutionRequest.CreateCacheRefreshRequest(
				willowEnvironment.Id,
				true,
				RulesOptions.ProcessorCloudRoleName,
				refreshSearchAfterwards: false,//we queue one manually after expansion
				refreshTwins: changes.hasTwinChanges,
				refreshRelationships: changes.hasRelationshipChanges);

			if (await repositoryRuleExecutionRequest.IsDuplicateRequest(request))
			{
				logger.LogWarning("ADT sync. A cache request is already queued. Skipping auto cache update");
				return;
			}

			await repositoryRuleExecutionRequest.UpsertOne(request);

			//2. then run expansion
			request = RuleExecutionRequest.CreateRuleExpansionRequest(
				willowEnvironment.Id,
				true,
				RulesOptions.ProcessorCloudRoleName);

			await repositoryRuleExecutionRequest.UpsertOne(request);

			//3. refresh search
			var searchRequest = RuleExecutionRequest.CreateSearchIndexRefreshRequest(
				willowEnvironment.Id,
				true,
				RulesOptions.ProcessorCloudRoleName);

			await repositoryRuleExecutionRequest.UpsertOne(searchRequest);
		}
	}

	private async Task<TimeZoneInfo> GetTimezone()
	{
		if (!string.IsNullOrEmpty(timeZone) && timeZone != "UTC")
		{
			return TimeZoneInfoHelper.From(timeZone);
		}

		try
		{
			var summary = await repositoryADTSummary.GetLatest();

			timeZone = summary.SystemSummary?.RuleInstanceSummary?.TimeZone;

			if (string.IsNullOrEmpty(timeZone))
			{
				timeZone = "UTC";
			}
		}
		catch (Exception ex)
		{
			timeZone = "UTC";
			logger.LogError(ex, "CacheSyncService Failed to get non UTC timezone");
		}

		return TimeZoneInfoHelper.From(timeZone);
	}
}

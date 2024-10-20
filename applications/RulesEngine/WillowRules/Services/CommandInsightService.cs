using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RulesEngine.Processor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Willow.RealEstate.Command.Generated;
using Willow.Rules.Configuration;
using Willow.Rules.Logging;
using Willow.Rules.Repository;
using WillowRules.Extensions;

namespace Willow.Rules.Services;

/// <summary>
/// Service for posting insights to Command
/// </summary>
public interface ICommandInsightService
{
	/// <summary>
	/// Tries to acquire a token
	/// </summary>
	Task TryAcquireToken();

	/// <summary>
	/// Post an insight to command
	/// </summary>
	Task<HttpStatusCode> UpsertInsightToCommand(Model.Insight insight);

	/// <summary>
	/// Deletes an insight from command
	/// </summary>
	Task<HttpStatusCode> DeleteInsightFromCommand(Model.Insight insight);

	/// <summary>
	/// Gets insights for a site id
	/// </summary>
	/// <returns></returns>
	Task<IEnumerable<InsightSimpleDto>> GetInsightsForSiteId(Guid siteId);

	/// <summary>
	///	Mark an insight as closed in Command
	/// </summary>
	Task<HttpStatusCode> CloseInsightInCommand(Model.Insight insight);
}

/// <summary>
/// Extensions for command objects
/// </summary>
public static class CommandExtensions
{
	/// <summary>
	/// Gets the insight status from a Commnad Insight
	/// </summary>
	public static Model.InsightStatus GetStatus(this InsightDetailDto value)
	{
		return value.LastStatus.GetStatus();
	}

	/// <summary>
	/// Gets the insight status from a Commnad Insight
	/// </summary>
	public static Model.InsightStatus GetStatus(this InsightSimpleDto value)
	{
		return value.LastStatus.GetStatus();
	}

	/// <summary>
	/// Gets the insight status from a Commnad Insight
	/// </summary>
	public static Model.InsightStatus GetStatus(this InsightStatus status)
	{
		switch (status)
		{
			case InsightStatus.Open:
				{
					return Model.InsightStatus.Open;
				}
			case InsightStatus.New:
				{
					return Model.InsightStatus.New;
				}
			case InsightStatus.InProgress:
				{
					return Model.InsightStatus.InProgress;
				}
			case InsightStatus.Resolved:
				{
					return Model.InsightStatus.Resolved;
				}
			case InsightStatus.Ignored:
				{
					return Model.InsightStatus.Ignored;
				}
		}

		return Model.InsightStatus.Open;
	}
}
/// <summary>
/// Service for posting insights to Command
/// </summary>
public class CommandInsightService : ICommandInsightService
{
	private const int MAX_DESCRIPTION_LENGTH = 4095;

	private readonly IHttpClientFactory httpClientFactory;
	private readonly IRepositoryInsightChange repositoryInsightChange;
	private readonly HealthCheckPublicAPI healthCheckPublicAPI;
	private readonly ILogger<CommandInsightService> logger;
	private readonly ILogger throttledLogger;
	private readonly ILogger throttledErrorLogger;
	private readonly RulesOptions options;
	private readonly ExponentialBackOff exponentialBackOff;
	private readonly bool autoSync;

	/// <summary>
	/// Creates a new <see cref="CommandInsightService" />
	/// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public CommandInsightService(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		IOptions<RulesOptions> rulesOptions,
		IOptions<CustomerOptions> customerOptions,
		IHttpClientFactory httpClientFactory,
		IRepositoryInsightChange repositoryInsightChange,
		HealthCheckPublicAPI healthCheckPublicAPI,
		ILogger<CommandInsightService> logger)
	{
		if (rulesOptions is null) throw new ArgumentNullException(nameof(rulesOptions));
		this.options = rulesOptions.Value ?? throw new ArgumentNullException(nameof(rulesOptions));

		this.healthCheckPublicAPI = healthCheckPublicAPI ?? throw new ArgumentNullException(nameof(healthCheckPublicAPI));

		if (this.options is null || this.options.PublicApi is null || string.IsNullOrWhiteSpace(this.options.PublicApi.Uri))
		{
			logger.LogWarning("Will not be able to post insights to command, PublicApi is not configured");
			this.httpClientFactory = null!;
			healthCheckPublicAPI.Current = HealthCheckPublicAPI.NotConfigured;
		}
		else
		{
			this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			healthCheckPublicAPI.Current = HealthCheckPublicAPI.Healthy;
		}

		this.repositoryInsightChange = repositoryInsightChange ?? throw new ArgumentNullException(nameof(repositoryInsightChange));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.throttledErrorLogger = logger.Throttle(TimeSpan.FromSeconds(30));
		this.throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
		this.exponentialBackOff = new ExponentialBackOff(5000, 2, 10 * 60 * 1000);   // 10 min max backoff getting a token																					 
		this.autoSync = false;// turned off for ddk for now -> customerOptions.Value.Id.Contains("ddk");//only sync ddk for now
	}

	private HttpClient GetClient()
	{
		if (httpClientFactory is null)
		{
			throw new ArgumentNullException(nameof(httpClientFactory));
		}

		var httpClient = httpClientFactory.CreateClient("CommandInsight");

		httpClient.BaseAddress = new Uri(this.options.PublicApi.Uri);

		return httpClient;
	}

	private AppAccountToken? appAccountToken = null;

	private DateTimeOffset nextRetry = DateTimeOffset.Now;

	/// <summary>
	/// Gets an authorized command client with rate limited retries
	/// </summary>
	private async Task<CommandClient?> GetAuthorizedCommandClient()
	{
		if (options is null || httpClientFactory is null)
		{
			throttledErrorLogger.LogWarning("Command Insights not configured");
			healthCheckPublicAPI.Current = HealthCheckPublicAPI.NotConfigured;
			return null;
		}

		if (appAccountToken is null)
		{
			try
			{
				// Too soon to retry?
				if (DateTimeOffset.Now < nextRetry)
				{
					throttledErrorLogger.LogWarning("Too soon to retry Insight Sync. Next retry at {time}", nextRetry);
					return null;
				}

				logger.LogInformation("Command Insights Url is configured as '{url}'", this.options.PublicApi.Uri);

				var client = new SecretClient(vaultUri: new Uri(this.options.KeyVaultUri),
					credential: new DefaultAzureCredential());

				// Retrieve a secret using the secret client.
				var clientId = client.GetSecret("publicapi-clientid");
				var clientSecret = client.GetSecret("publicapi-clientsecret");

				var commandClient = new CommandClient(GetClient());

				var signInRequest = new SignInRequest
				{
					ClientId = clientId.Value.Value,
					ClientSecret = clientSecret.Value.Value
				};

				appAccountToken = await commandClient.TokenAsync(signInRequest);
				exponentialBackOff.Reset();
				logger.LogInformation($"Command Insights Got app token");
				healthCheckPublicAPI.Current = HealthCheckPublicAPI.Healthy;

				commandClient.SetAccessToken(appAccountToken.AccessToken);
				return commandClient;
			}
			catch (ApiException apiEx) when (apiEx.StatusCode == 429)
			{
				logger.LogInformation(apiEx, "Command Insights Rate limited by Auth0");
				nextRetry = nextRetry.AddMilliseconds(exponentialBackOff.GetNextDelay());
				healthCheckPublicAPI.Current = HealthCheckPublicAPI.RateLimited;
				return null;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Command Insights failed to get app token");
				nextRetry = nextRetry.AddMilliseconds(exponentialBackOff.GetNextDelay());
				healthCheckPublicAPI.Current = HealthCheckPublicAPI.FailingCalls;
				return null;
			}
		}
		else
		{
			CommandClient commandClient = new(GetClient());
			commandClient.SetAccessToken(appAccountToken.AccessToken);
			return commandClient;
		}
	}

	public async Task<HttpStatusCode> DeleteInsightFromCommand(Model.Insight insight)
	{
		using var disp = logger.BeginScope(new Dictionary<string, object> { ["Insight"] = insight.Id });

		try
		{
			CommandClient? commandClient = await GetAuthorizedCommandClient();
			if (commandClient is null)
			{
				throttledErrorLogger.LogWarning("Command Insights Could not get an AppToken to delete from InsightsCore PublicAPI");
				return HttpStatusCode.ServiceUnavailable;
			}

			if (insight.SiteId is null) { logger.LogError("Could not delete insight from Command, missing SiteId"); return HttpStatusCode.BadRequest; }
			if (insight.CommandInsightId == Guid.Empty) { logger.LogError("Could not delete insight from Command, missing CommandInsightId"); return HttpStatusCode.BadRequest; }

			Guid siteId = insight.SiteId ?? Guid.Empty;

			var token = new CancellationTokenSource(TimeSpan.FromMinutes(1));

			await commandClient.InsightsDELETEAsync(siteId, insight.CommandInsightId, token.Token);

			logger.LogInformation("Command Insight deleted {commandInsightId}", insight.CommandInsightId);

			healthCheckPublicAPI.Current = HealthCheckPublicAPI.Healthy;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 500)
		{
			logger.LogWarning(apiex, "Failed to delete insight from Command");
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 404)
		{
			logger.LogWarning(apiex, "Failed to delete insight from Command, insight not found {commandId}", insight.CommandInsightId);
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 401)
		{
			logger.LogWarning(apiex, "Failed to delete insight from Command, not authenticated, should stop trying {commandId}", insight.CommandInsightId);
			this.appAccountToken = null;  // reset the app token, try auth again - should be only on a 401/403 error
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 403)
		{
			throttledErrorLogger.LogWarning(apiex, "Failed to delete insight from Command, not authorized, should stop trying {commandId}", insight.CommandInsightId);
			//the  "cannot access site" is a site specific error, don't stop future calls
			if (!apiex.Message.Contains("cannot access site"))
			{
				this.appAccountToken = null;  // reset the app token, try auth again - should be only on a 401/403 error
				nextRetry = DateTimeOffset.Now.AddMinutes(30);
			}
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 204)
		{
			throttledLogger.LogInformation("Command Id {id} has been deleted. Status 204", insight.CommandInsightId);
		}
		catch (ApiException apiex)
		{
			logger.LogWarning(apiex, "Failed to delete insight from Command, status={code} {commandId}", apiex.StatusCode, insight.CommandInsightId);
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to delete insight from Command {commandId}", insight.CommandInsightId);
			this.appAccountToken = null;  // reset the app token, try auth again - should be only on a 401/403 error
			return HttpStatusCode.InternalServerError;
		}

		return HttpStatusCode.OK;
	}

	public async Task<IEnumerable<InsightSimpleDto>> GetInsightsForSiteId(Guid siteId)
	{
		using (logger.TimeOperation(TimeSpan.FromSeconds(30), "Getting Insights for site id {siteId}", siteId))
		{
			CommandClient? commandClient = await GetAuthorizedCommandClient();
			if (commandClient is null)
			{
				throttledErrorLogger.LogWarning("Command Insights. Could not get token to get insights.");
				return Array.Empty<InsightSimpleDto>();
			}

			try
			{
				return await commandClient.InsightsAllAsync(siteId, true);
			}
			catch (ApiException apiex) when (apiex.StatusCode == 500)
			{
				logger.LogWarning(apiex, "Failed to get insights");
			}
			catch (ApiException apiex) when (apiex.StatusCode == 401)
			{
				logger.LogWarning(apiex, "Failed to get insights, not authenticated, should stop trying");
				this.appAccountToken = null;  // reset the app token, try auth again - should be only on a 401/403 error
			}
			catch (ApiException apiex) when (apiex.StatusCode == 403)
			{
				throttledErrorLogger.LogWarning(apiex, "Failed to get insights, not authorized, should stop trying");
				//the  "cannot access site" is a site specific error, don't stop future calls
				if (!apiex.Message.Contains("cannot access site"))
				{
					this.appAccountToken = null;  // reset the app token, try auth again - should be only on a 401/403 error
					nextRetry = DateTimeOffset.Now.AddMinutes(30);
				}
			}
			catch (ApiException apiex)
			{
				logger.LogWarning(apiex, "Failed to get insights, status={code}", apiex.StatusCode);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to get insights");
				this.appAccountToken = null;  // reset the app token, try auth again - should be only on a 401/403 error
			}
		}

		return Array.Empty<InsightSimpleDto>();
	}

	public async Task<HttpStatusCode> UpsertInsightToCommand(Model.Insight insight)
	{
		// 1 == Urgent, 2 == High, 3 == Medium, 4 == Low
		int priority = insight.IsFaulty ? 2 : insight.IsValid ? 4 : 3;

		using var disp = logger.BeginScope(new Dictionary<string, object>
		{
			["Insight"] = insight.Id,
			["CommandId"] = insight.CommandInsightId
		});

		CommandClient? commandClient = await GetAuthorizedCommandClient();

		if (commandClient is null)
		{
			throttledErrorLogger.LogWarning("Command Insights. Could not upsert, no Command Client.");
			//return OK then at least UI sync shows green when client not configured
			return HttpStatusCode.OK;
		}

		try
		{
			if (insight.SiteId is null || insight.SiteId == Guid.Empty)
			{
				logger.LogError("Could not send insight to Command, missing SiteId");
				return HttpStatusCode.BadRequest;
			}

			Guid siteId = insight.SiteId ?? Guid.Empty;
			Guid? uniqueEquipmentId = insight.EquipmentUniqueId;

			// Try filling in the Unique equipment Id by calling Command
			if (uniqueEquipmentId is null || uniqueEquipmentId == Guid.Empty)
			{
				if (insight.SiteId is Guid si && insight.EquipmentId is string equipmentId)
				{
					var twn = await commandClient.GetTwinAsync(si, equipmentId);

					logger.LogInformation("Twin from Command {twn.Id}", twn.Id);

					if (twn.AdditionalProperties.TryGetValue("uniqueID", out object? uid))
					{
						if (uid is Guid g)
						{
							if (uniqueEquipmentId is Guid g1 && !g.Equals(g1)) logger.LogWarning("Unique equipment Id did not agreee {g1} {g2}", g1, g);
							uniqueEquipmentId = g;
						}
						else if (uid is string s && Guid.TryParse(s, out Guid g2))
						{
							if (uniqueEquipmentId is Guid g1 && !g2.Equals(g1)) logger.LogWarning("Unique equipment Id did not agreee {g1} {g2}", g1, g2);
							uniqueEquipmentId = g2;
						}

						// Save the value back on the insight
						insight.EquipmentUniqueId = uniqueEquipmentId;
					}
				}
			}

			string name = insight.RuleName + " " + insight.EquipmentId;
			var firstOccurrence = insight.EarliestFaultedDate;
			var lastOccurrence = insight.LastFaultedDate;
			var occurrenceCount = insight.FaultedCount;

			var scores = insight.ImpactScores.Select(v => new ImpactScore()
			{
				FieldId = v.FieldId,
				Name = v.Name,
				Unit = v.Unit ?? string.Empty, //value is required on command so default back to empty string
				Value = v.Score,
				ExternalId = v.ExternalId
			}).ToList();

			var occurrences = insight.Occurrences?.Select(x => new InsightOccurrence()
			{
				OccurrenceId = x.Id,
				IsFaulted = x.IsFaulted,
				IsValid = x.IsValid,
				Started = x.Started,
				Ended = x.Ended,
				Text = x.Text
			}).ToList();

			var dependencies = insight.Dependencies.Where(v => v.CommandInsightId != Guid.Empty)
				.Select(v => new Dependency() { InsightId = v.CommandInsightId, Relationship = v.Relationship }).ToList();

			var points = insight.Points.Select(v => new Point() { TwinId = v.Id }).ToList();

			var locations = insight.TwinLocations.Select(v => v.Id).ToList();

			if (insight.CommandInsightId == Guid.Empty)
			{
				// First time push, need to have external Id etc.
				var body = new CreateInsightRequest
				{
					FloorCode = "",
					EquipmentId = uniqueEquipmentId,
					Type = InsightTypeFromCategory(insight.RuleCategory),
					Name = name,
					Description = insight.Text.LimitWithEllipses(MAX_DESCRIPTION_LENGTH),
					Priority = priority,
					State = InsightState.Active,
					OccurredDate = lastOccurrence,
					OccurrenceCount = occurrenceCount,
					DetectedDate = firstOccurrence,
					ExternalId = insight.Id,
					ExternalStatus = $"{insight.Invocations} invocations",
					ExternalMetadata = "",
					Recommendation = insight.RuleRecomendations,
					ImpactScores = scores,
					RuleId = insight.RuleId,
					RuleName = insight.RuleName,
					TwinId = insight.EquipmentId,
					PrimaryModelId = insight.PrimaryModelId,
					InsightOccurrences = occurrences,
					Dependencies = dependencies,
					Points = points,
					Locations = locations
				};

				var insightDetailDto = await commandClient.InsightsPOSTAsync(siteId, body);
				Guid commandInsightId = insightDetailDto.Id;

				logger.LogInformation("Insight posted {commandInsightId}", commandInsightId);

				//This necessary when we gonna persist on background service?
				//await repositoryInsight.SetCommandInsightId(insight.Id, commandInsightId, insightDetailDto.GetStatus());

				logger.LogInformation("Guid from command recorded {commandInsightId}", commandInsightId);

				insight.InsightSynced(insightDetailDto.GetStatus(), commandInsightId);
			}
			else
			{
				bool currentlyFaulty = insight.IsFaulty;

				// keep as null by default otherwise we keep reopening it after it's been cleared in Willow App
				InsightStatus? status = null;

				//we can remove this flag once auto sync should occur for all customers. Currently only for ddk
				//only allow status changes once we have passed the next allowed sync date
				if (autoSync && DateTime.UtcNow > insight.NextAllowedSyncDateUTC)
				{
					//the logic needs to align with insight service validation
					//validation can be found at /extensions/real-estate/back-end/InsightCore/src/InsightCore/Services/InsightService.cs
					//auto resolve if the insight isn't faulty anymore
					if (insight.CanResolve())
					{
						status = InsightStatus.Resolved;
					}
					//auto open if insight is currently faulty
					else if (insight.CanReOpen())
					{
						status = InsightStatus.New;
					}
				}

				if (status is not null)
				{
					//the insight service has status validation. If the status is not updating please check error logs
					logger.LogInformation("Changing insight status from {s1} to {s2} for insight {commandInsightId}", insight.Status.ToString(), status.ToString(), insight.CommandInsightId);
				}

				var updateRequest = new UpdateInsightRequest
				{
					Name = name,
					Description = insight.Text.LimitWithEllipses(MAX_DESCRIPTION_LENGTH),
					Type = InsightTypeFromCategory(insight.RuleCategory),
					Priority = priority,
					State = null,
					LastStatus = status,
					OccurredDate = lastOccurrence > DateTime.MinValue ? lastOccurrence : null,
					OccurrenceCount = occurrenceCount,
					ExternalId = insight.Id,
					ExternalStatus = $"{insight.Invocations} invocations",
					ExternalMetadata = "",
					Recommendation = insight.RuleRecomendations,
					ImpactScores = scores,
					PrimaryModelId = insight.PrimaryModelId,
					InsightOccurrences = occurrences,
					RuleName = insight.RuleName,
					Dependencies = dependencies,
					Points = points,
					Locations = locations
				};

				var commandResult = await commandClient.InsightsPUTAsync(siteId, insight.CommandInsightId, updateRequest);

				var newStatus = commandResult.GetStatus();

				bool statusChanged = newStatus != insight.Status;

				insight.InsightSynced(newStatus, insight.CommandInsightId);

				//This necessary when we gonna persist on background service?
				//int updateCount = await repositoryInsight.SetCommandInsightId(insight.Id, commandResult.Id, commandResult.GetStatus());

				throttledLogger.LogInformation("Command Insight updated {commandInsightId} {updatedDate} {priority}", insight.CommandInsightId, commandResult.UpdatedDate, priority);

				//performance: only get status changes if the insight is in the db and the command insight is active
				//this should be replaced by service bus one day
				if (statusChanged && commandResult.State == InsightState.Active)
				{
					var statsLogs = await commandClient.StatusLogAsync(siteId, insight.CommandInsightId);
					var changes = statsLogs.Select(v => new Model.InsightChange(insight.Id, v.Status.GetStatus(), v.CreatedDateTime)).ToList();
					await repositoryInsightChange.OverwriteInsightChanges(insight, changes);
				}

				healthCheckPublicAPI.Current = HealthCheckPublicAPI.Healthy;
				// delete an insight? await commandClient.InsightsDELETEAsync(siteId, insight.CommandInsightId);
			}

			return HttpStatusCode.OK;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 500)
		{
			healthCheckPublicAPI.Current = HealthCheckPublicAPI.FailingCalls;
			logger.LogError(apiex, "Failed to post insight to Command");
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 401)
		{
			healthCheckPublicAPI.Current = HealthCheckPublicAPI.NotAuthorized;
			logger.LogWarning(apiex, "Failed to post insight to Command, not authenticated, should stop trying");
			this.appAccountToken = null;  // reset the app token, try auth again - should be only on a 401/403 error
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 403)
		{
			healthCheckPublicAPI.Current = HealthCheckPublicAPI.NotAuthorized;
			//the  "cannot access site" is a site specific error, don't stop future calls
			if (!apiex.Message.Contains("cannot access site"))
			{
				this.appAccountToken = null;  // reset the app token, try auth again - should be only on a 401/403 error
				nextRetry = DateTimeOffset.Now.AddMinutes(30);
			}
			throttledErrorLogger.LogWarning(apiex, "Failed to post insight to Command, not authorized, should stop trying");
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 404)
		{
			// "Service InsightCore returns failure (NotFound). Resource(insight: 7580decf-6447-42eb-9323-94075d49c304) cannot be found"
			logger.LogInformation("Failed to upsert insight to Command, 404, marking it not synced. {message}", apiex.Message);
			insight.CommandInsightId = Guid.Empty;
			insight.CommandEnabled = false;
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 400)
		{
			// "Service InsightCore returns bad request for changing to an incorrect status
			logger.LogInformation("Failed to upsert insight to Command, 400. {message}", apiex.Message);
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 204)
		{
			logger.LogInformation("Command Id {id} has been deleted. Status 204. {message}", insight.CommandInsightId, apiex.Message);
			//insight should get recreated on next run. 204 indicates deleted
			insight.CommandInsightId = Guid.Empty;
			insight.Status = Model.InsightStatus.Deleted;
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex)
		{
			logger.LogWarning(apiex, "Failed to post insight to Command, status={code}", apiex.StatusCode);
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to post insight to Command");
			this.appAccountToken = null;  // reset the app token, try auth again - should be only on a 401/403 error
			return HttpStatusCode.InternalServerError;
		}

		// TODO: Update the insight so we can see in the UI that it's failing to sync to Command
	}

	/// <summary>
	/// Converts between the free-form rules engine category and the enum used by Real-Estate
	/// </summary>
	private static InsightType InsightTypeFromCategory(string category)
	{
		Dictionary<string, InsightType> categoryMap = new(StringComparer.OrdinalIgnoreCase) {
			{ "Alarm", InsightType.Alert },
			{ "Alert", InsightType.Alert },
			{ "Comfort", InsightType.Comfort },
			{ "Commissioning", InsightType.Commissioning },
			{ "Data quality", InsightType.DataQuality },
			{ "Diagnostic", InsightType.Diagnostic },
			{ "Energy", InsightType.Energy },
			{ "Fault", InsightType.Fault },
			{ "Note", InsightType.Note },
			{ "Predictive", InsightType.Predictive }
		};

		if (categoryMap.TryGetValue(category, out var insightType))
		{
			return insightType;
		}

		return InsightType.Fault;
	}

	public async Task<HttpStatusCode> CloseInsightInCommand(Model.Insight insight)
	{
		// acce2181-e847-442b-98f7-fbcc70d4d584
		Guid siteId = insight.SiteId ?? Guid.Empty;

		using var disp = logger.BeginScope(new Dictionary<string, object> { ["Insight"] = insight.Id });

		CommandClient? commandClient = await GetAuthorizedCommandClient();
		if (commandClient is null)
		{
			throttledErrorLogger.LogWarning("Command Insights. Could not close insight in Command.");
			return HttpStatusCode.ServiceUnavailable;
		}

		try
		{
			if (insight.SiteId is null) { logger.LogError("Could not update insight in Command, missing SiteId"); return HttpStatusCode.BadRequest; }
			if (insight.CommandInsightId == Guid.Empty) { logger.LogError("Could not update insight in Command, missing CommandInsightId"); return HttpStatusCode.BadRequest; }

			var updateRequest = new UpdateInsightRequest
			{
				Priority = 4,  // 2=High, 3=Medium, 4=Low
				State = InsightState.Archived,
				LastStatus = InsightStatus.Ignored
			};

			await commandClient.InsightsPUTAsync(siteId, insight.CommandInsightId, updateRequest);

			logger.LogInformation("Insight closed in Command, set Priority, Archived, Closed {id} {commandId}", insight.Id, insight.CommandInsightId);

			healthCheckPublicAPI.Current = HealthCheckPublicAPI.Healthy;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 500)
		{
			logger.LogWarning(apiex, "Failed to udpate insight in Command");
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 404)
		{
			logger.LogInformation("Failed to update insight in Command, insight not found {commandId}", insight.CommandInsightId);
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 401)
		{
			logger.LogWarning(apiex, "Failed to update insight in Command, not authenticated, should stop trying {commandId}", insight.CommandInsightId);
			this.appAccountToken = null;  // reset the app token, try auth again - should be only on a 401/403 error
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 403)
		{
			throttledErrorLogger.LogWarning(apiex, "Failed to update insight in Command, not authorized, should stop trying {commandId}", insight.CommandInsightId);
			//the  "cannot access site" is a site specific error, don't stop future calls
			if (!apiex.Message.Contains("cannot access site"))
			{
				this.appAccountToken = null;  // reset the app token, try auth again - should be only on a 401/403 error
				nextRetry = DateTimeOffset.Now.AddMinutes(30);
			}
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (ApiException apiex) when (apiex.StatusCode == 204)
		{
			throttledLogger.LogInformation("Command Id {id} has been deleted. Status 204", insight.CommandInsightId);
		}
		catch (ApiException apiex)
		{
			logger.LogWarning(apiex, "Failed to update insight in Command, status={code} {commandId}", apiex.StatusCode, insight.CommandInsightId);
			return (HttpStatusCode)apiex.StatusCode;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to update insight in Command {commandId}", insight.CommandInsightId);
			this.appAccountToken = null;  // reset the app token, try auth again - should be only on a 401/403 error
			return HttpStatusCode.InternalServerError;
		}

		return HttpStatusCode.OK;
	}

	public async Task TryAcquireToken()
	{
		await GetAuthorizedCommandClient();
	}
}

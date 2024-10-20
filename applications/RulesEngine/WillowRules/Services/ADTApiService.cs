using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using RulesEngine.Processor.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.DataQuality.Model.Capability;

namespace Willow.Rules.Services;

public interface IADTApiService
{
	/// <summary>
	/// Try and get an Authorized Http Client which will health check the ADT Api Service
	/// </summary>
	Task TryGetTwinsCount();

	/// <summary>
	/// Configuration check that can be used by consumers
	/// </summary>
	bool IsConfiguredCorrectly { get; }

	/// <summary>
	/// Service call to send capability status updates to ADT
	/// </summary>
	Task CreateStatusAsync(IEnumerable<CapabilityStatusDto> status, CancellationToken cancellationToken = default);

	/// <summary>
	/// Service call to upsert twins in ADT
	/// </summary>
	Task<BasicDigitalTwin?> UpdateTwinAsync(BasicDigitalTwin twin, bool? includeAdxUpdate = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Service call to upsert a twin relationship in ADT
	/// </summary>
	Task<BasicRelationship?> UpsertRelationshipAsync(BasicRelationship relationship, CancellationToken cancellationToken = default);

	/// <summary>
	/// Service call to delete twins and their relationships in ADT
	/// </summary>
	Task<MultipleEntityResponse> DeleteTwinsAndRelationshipsAsync(IEnumerable<string> twinIds, bool? deleteRelationships = null, CancellationToken cancellationToken = default);
}

public class ADTApiService : IADTApiService
{
	private readonly ITwinsClient? twinsClient;
	private readonly IRelationshipsClient? relationshipClient;
	private readonly IDQCapabilityClient? dqCapabilityClient;
	private readonly HealthCheckADTApi healthCheckADTApi;
	private readonly ILogger<ADTApiService> logger;

	public ADTApiService(
		HealthCheckADTApi healthCheckADTApi,
		ILogger<ADTApiService> logger,
		ITwinsClient? twinsClient = null,
		IRelationshipsClient? relationshipClient = null,
		IDQCapabilityClient? dqCapabilityClient = null)
	{
		this.twinsClient = twinsClient;
		this.relationshipClient = relationshipClient;
		this.dqCapabilityClient = dqCapabilityClient;

		this.healthCheckADTApi = healthCheckADTApi ?? throw new ArgumentNullException(nameof(healthCheckADTApi));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

		if (IsConfiguredCorrectly)
		{
			healthCheckADTApi.Current = HealthCheckADTApi.Healthy;
		}
		else
		{
			logger.LogWarning("The ADT Api Service is not configured correctly.");
			healthCheckADTApi.Current = HealthCheckADTApi.NotConfigured;
		}
	}

	public bool IsConfiguredCorrectly
	{
		get { return twinsClient is not null && relationshipClient is not null && dqCapabilityClient is not null; }
	}

	/// <summary>
	/// Try and get a Twins Count which will health check the ADT Api Service
	/// </summary>
	public async Task TryGetTwinsCount()
	{
		if (IsConfiguredCorrectly)
		{
			await twinsClient!.GetTwinsCountAsync();
		}
		else
		{
			healthCheckADTApi.Current = HealthCheckADTApi.NotConfigured;
		}
	}

	/// <summary>
	/// Service call to send capability status updates to ADT
	/// </summary>
	/// <param name="status"></param>
	/// <param name="cancellationToken"></param>
	public async Task CreateStatusAsync(IEnumerable<CapabilityStatusDto> status, CancellationToken cancellationToken = default)
	{
		try
		{
			if (dqCapabilityClient is not null)
			{
				await dqCapabilityClient.CreateStatusAsync(status, cancellationToken);

				healthCheckADTApi.Current = HealthCheckADTApi.Healthy;
			}
			else
			{
				logger.LogError("The ADT Data Quality Client is not configured.");
				healthCheckADTApi.Current = HealthCheckADTApi.NotConfigured;
			}
		}
		catch (ApiException apiex) when (apiex.StatusCode == 401)
		{
			logger.LogError(apiex, "Failed to SendCapabilityStatusUpdate, not authorized, status={code}", apiex.StatusCode);
			healthCheckADTApi.Current = HealthCheckADTApi.AuthorizationFailure;
			throw;
		}
		catch (ApiException apiex)
		{
			logger.LogError(apiex, "Failed to SendCapabilityStatusUpdate, status={code}", apiex.StatusCode);
			healthCheckADTApi.Current = HealthCheckADTApi.FailingCalls;
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to SendCapabilityStatusUpdate");
			throw;
		}
	}

	/// <summary>
	/// Service call to upsert twins in ADT
	/// </summary>
	/// <param name="twin"></param>
	/// <param name="includeAdxUpdate"></param>
	/// <param name="cancellationToken"></param>
	public async Task<BasicDigitalTwin?> UpdateTwinAsync(BasicDigitalTwin twin, bool? includeAdxUpdate = null, CancellationToken cancellationToken = default)
	{
		try
		{
			if (twinsClient is not null)
			{
				var updatedTwin = await twinsClient.UpdateTwinAsync(twin, includeAdxUpdate: true, cancellationToken: cancellationToken);

				healthCheckADTApi.Current = HealthCheckADTApi.Healthy;

				return updatedTwin;
			}
			else
			{
				logger.LogError("The ADT Twins Client is not configured.");
				healthCheckADTApi.Current = HealthCheckADTApi.NotConfigured;
			}
		}
		catch (ApiException apiex) when (apiex.StatusCode == 401)
		{
			logger.LogError(apiex, "Failed to UpdateTwinAsync, not authorized, status={code}, twin={twinId}", apiex.StatusCode, twin.Id);
			healthCheckADTApi.Current = HealthCheckADTApi.AuthorizationFailure;
			throw;
		}
		catch (ApiException apiex)
		{
			logger.LogError(apiex, "Failed to UpdateTwinAsync, status={code}, twin={twinId}", apiex.StatusCode, twin.Id);
			healthCheckADTApi.Current = HealthCheckADTApi.FailingCalls;
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to UpdateTwinAsync, twin={twinId}", twin.Id);
			throw;
		}

		return null;
	}

	/// <summary>
	/// Service call to upsert a twin relationship in ADT
	/// </summary>
	/// <param name="relationship"></param>
	/// <param name="cancellationToken"></param>
	public async Task<BasicRelationship?> UpsertRelationshipAsync(BasicRelationship relationship, CancellationToken cancellationToken = default)
	{
		try
		{
			if (relationshipClient is not null)
			{
				var upsertedRelationship = await relationshipClient.UpsertRelationshipAsync(relationship, cancellationToken);

				healthCheckADTApi.Current = HealthCheckADTApi.Healthy;

				return upsertedRelationship;
			}
			else
			{
				logger.LogError("The ADT Relationship Client is not configured.");
				healthCheckADTApi.Current = HealthCheckADTApi.NotConfigured;
			}
		}
		catch (ApiException apiex) when (apiex.StatusCode == 401)
		{
			logger.LogError(apiex, "Failed to UpsertRelationshipAsync, not authorized, status={code}, rel={relId}", apiex.StatusCode, relationship.Id);
			healthCheckADTApi.Current = HealthCheckADTApi.AuthorizationFailure;
			throw;
		}
		catch (ApiException apiex)
		{
			logger.LogError(apiex, "Failed to UpsertRelationshipAsync, status={code}, rel={relId}", apiex.StatusCode, relationship.Id);
			healthCheckADTApi.Current = HealthCheckADTApi.FailingCalls;
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to UpsertRelationshipAsync, rel={relId}", relationship.Id);
			throw;
		}

		return null;
	}

	/// <summary>
	/// Service call to delete twins and their relationships in ADT
	/// </summary>
	/// <param name="twinIds"></param>
	/// <param name="deleteRelationships"></param>
	/// <param name="cancellationToken"></param>
	public async Task<MultipleEntityResponse> DeleteTwinsAndRelationshipsAsync(IEnumerable<string> twinIds, bool? deleteRelationships = null, CancellationToken cancellationToken = default)
	{
		try
		{
			if (twinsClient is not null)
			{
				var response = await twinsClient.DeleteTwinsAndRelationshipsAsync(twinIds, deleteRelationships: true, cancellationToken: cancellationToken);

				healthCheckADTApi.Current = HealthCheckADTApi.Healthy;

				return response;
			}
			else
			{
				logger.LogError("The ADT Twins Client is not configured.");
				healthCheckADTApi.Current = HealthCheckADTApi.NotConfigured;
				return new MultipleEntityResponse();
			}
		}
		catch (ApiException apiex) when (apiex.StatusCode == 401)
		{
			logger.LogError(apiex, "Failed to DeleteTwinsAndRelationshipsAsync, not authorized, status={code}", apiex.StatusCode);
			healthCheckADTApi.Current = HealthCheckADTApi.AuthorizationFailure;
			throw;
		}
		catch (ApiException apiex)
		{
			logger.LogError(apiex, "Failed to DeleteTwinsAndRelationshipsAsync, status={code}", apiex.StatusCode);
			healthCheckADTApi.Current = HealthCheckADTApi.FailingCalls;
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to DeleteTwinsAndRelationshipsAsync");
			throw;
		}
	}
}

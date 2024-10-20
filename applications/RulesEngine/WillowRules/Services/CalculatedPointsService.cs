using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Extensions.Logging;
using Willow.Rules.Cache;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;

namespace WillowRules.Services
{
	/// <summary>
	/// Service to process calculated points in ADT
	/// </summary>
	public interface ICalculatedPointsService
	{
		/// <summary>
		/// Process calculated points to create, update or delete capability twins in ADT
		/// </summary>
		Task<(int updated, int deleted)> ProcessCalculatedPoints(
			IEnumerable<CalculatedPoint> calculatedPoints, ProgressTracker tracker, CancellationToken cancellationToken = default);
	}

	public class CalculatedPointsService : ICalculatedPointsService
	{
		private readonly IRepositoryCalculatedPoint repositoryCalculatedPoints;
		private readonly IRepositoryRules repositoryRules;
		private readonly IADTApiService adtApiService;
		private readonly ILogger<CalculatedPointsService> logger;
		private readonly IDataCacheFactory dataCacheFactory;
		private readonly WillowEnvironment willowEnvironment;

		/// <summary>
		/// Service to process calculated points in ADT
		/// </summary>
		/// <param name="repositoryCalculatedPoints"></param>
		/// <param name="repositoryRules"></param>
		/// <param name="adtApiService"></param>
		/// <param name="logger"></param>
		public CalculatedPointsService(
			IRepositoryCalculatedPoint repositoryCalculatedPoints,
			IRepositoryRules repositoryRules,
			IADTApiService adtApiService,
			IDataCacheFactory dataCacheFactory,
			WillowEnvironment willowEnvironment,
			ILogger<CalculatedPointsService> logger
			)
		{
			this.repositoryCalculatedPoints = repositoryCalculatedPoints ?? throw new ArgumentNullException(nameof(repositoryCalculatedPoints));
			this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
			this.adtApiService = adtApiService ?? throw new ArgumentNullException(nameof(adtApiService));
			this.dataCacheFactory = dataCacheFactory ?? throw new ArgumentNullException(nameof(dataCacheFactory));
			this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Manage calculated point twins in ADT
		/// </summary>
		/// <param name="calculatedPoints"></param>
		/// <param name="tracker"></param>
		/// <param name="cancellationToken"></param>
		public async Task<(int updated, int deleted)> ProcessCalculatedPoints(IEnumerable<CalculatedPoint> calculatedPoints, ProgressTracker tracker, CancellationToken cancellationToken = default)
		{
			var updatedCount = 0;
			var deletedCount = 0;

			try
			{
				var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(5));

				var rulesLookup = await repositoryRules.Get(r => r.TemplateId == RuleTemplateCalculatedPoint.ID);
				var calculatedPointsToUpsert = calculatedPoints.Where(cp => cp.ActionRequired == ADTActionRequired.Upsert);
				var calculatedPointsToDelete = calculatedPoints.Where(cp => cp.ActionRequired == ADTActionRequired.Delete);
				bool isConfigured = adtApiService.IsConfiguredCorrectly;

				if (calculatedPointsToUpsert.Any())
				{
					foreach (var calculatedPoint in calculatedPointsToUpsert)
					{
						try
						{
							var cpRule = rulesLookup.FirstOrDefault(r => r.Id == calculatedPoint.RuleId);

							if (cpRule != null && cpRule.ADTEnabled)
							{
								if (calculatedPoint.SiteId.HasValue && calculatedPoint.SiteId != Guid.Empty)
								{
									if (isConfigured)
									{
										var cpTwinResponse = await adtApiService.UpdateTwinAsync(CreateDigitalTwin(calculatedPoint), includeAdxUpdate: true, cancellationToken: cancellationToken);

										var cpRelationshipResponse = await adtApiService.UpsertRelationshipAsync(new BasicRelationship()
										{
											Id = $"{calculatedPoint.Id}_{calculatedPoint.IsCapabilityOf}",
											Name = "isCapabilityOf",
											SourceId = calculatedPoint.Id,
											TargetId = calculatedPoint.IsCapabilityOf
										}, cancellationToken);
									}

									//force a refresh in the cache when access from the UI,
									//the output external id may have changed
									(_, var twin) = await dataCacheFactory.TwinCache.TryGetValue(willowEnvironment.Id, calculatedPoint.Id);

									if(twin is not null)
									{
										twin.externalID = calculatedPoint.ExternalId;

										await dataCacheFactory.TwinCache.AddOrUpdate(willowEnvironment.Id, twin.Id, twin);
									}

									throttledLogger.LogInformation("Upsert ADT Twin {twinId}, with isCapabilityOf target {IsCapabilityOfId}, for rule {ruleId}.", calculatedPoint.Id, calculatedPoint.IsCapabilityOf, cpRule.Id);

									calculatedPoint.ActionStatus = ADTActionStatus.TwinAvailable;
									calculatedPoint.LastSyncDateUTC = DateTime.UtcNow;

									updatedCount++;

									await tracker.SetValues("Upserted", updatedCount, calculatedPointsToUpsert.Count(), isIgnored: false, force: true);
								}
								else
								{
									calculatedPoint.ActionStatus = ADTActionStatus.Failed;
									logger.LogWarning("Calculated point {calculatedPoint} siteId is null or empty guid", calculatedPoint.Name);
								}
							}
						}
						catch (ApiException apiex) when (apiex.StatusCode == 401)
						{
							calculatedPoint.ActionStatus = ADTActionStatus.Failed;
							break;
						}
						catch (Exception ex)
						{
							calculatedPoint.ActionStatus = ADTActionStatus.Failed;
							logger.LogError(ex, $"Failed to process calculated point {calculatedPoint.Id}");
						}
						finally
						{
							await repositoryCalculatedPoints.QueueWrite(calculatedPoint);
						}
					}
				}

				if (calculatedPointsToDelete.Any())
				{
					var totalCount = calculatedPointsToDelete.Count();

					if (isConfigured)
					{
						foreach (var batch in calculatedPointsToDelete.Chunk(20))
						{
							try
							{
								var response = await adtApiService.DeleteTwinsAndRelationshipsAsync(batch.Select(cp => cp.Id), deleteRelationships: true, cancellationToken: cancellationToken);

								var deletedEntityIds = response.Responses.Where(r => r.StatusCode == HttpStatusCode.OK || r.StatusCode == HttpStatusCode.NotFound).Select(e => e.EntityId);
								var cpsToDelete = batch.Where(cp => deletedEntityIds.Contains(cp.Id)).ToList();
								if (cpsToDelete.Count != 0)
								{
									await repositoryCalculatedPoints.BulkDelete(cpsToDelete, cancellationToken: cancellationToken);
									deletedCount += cpsToDelete.Count;
									throttledLogger.LogInformation($"Deleted {deletedCount} / {calculatedPointsToDelete.Count()} ADT Twins and relationships");
									await tracker.SetValues("Deleted", deletedCount, totalCount, isIgnored: false, force: true);
								}

								if (deletedEntityIds.Count() < batch.Length)
								{
									var remainingEntities = batch.Where(cp => !deletedEntityIds.Contains(cp.Id));

									remainingEntities.ToList().ForEach(async cp =>
									{
										cp.ActionStatus = ADTActionStatus.Failed;
										await repositoryCalculatedPoints.QueueWrite(cp);
										logger.LogWarning($"Failed to delete calculated point {cp.Id} with status code {response.Responses.FirstOrDefault(r => r.EntityId == cp.Id)?.StatusCode}");
									});
								}
							}
							catch (OperationCanceledException)
							{
								throw;
							}
							catch (ApiException apiex) when (apiex.StatusCode == 401)
							{
								batch.ToList().ForEach(async cp => { cp.ActionStatus = ADTActionStatus.Failed; await repositoryCalculatedPoints.QueueWrite(cp); });
								break;
							}
							catch (Exception ex)
							{
								throttledLogger.LogError(ex, "Failed to delete calculated points batch");

								batch.ToList().ForEach(async cp => { cp.ActionStatus = ADTActionStatus.Failed; await repositoryCalculatedPoints.QueueWrite(cp); });
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				await tracker.Failed();
				logger.LogError(ex, $"Failed to process calculated points, message={ex.Message}");
				throw;
			}
			finally
			{
				await repositoryCalculatedPoints.FlushQueue();
			}

			return (updatedCount, deletedCount);
		}

		private static BasicDigitalTwin CreateDigitalTwin(CalculatedPoint calculatedPoint)
		{
#pragma warning disable CS8601 // Possible null reference assignment.
			BasicDigitalTwin? newTwin = new()
			{
				Id = calculatedPoint.Id,
				Metadata = new DigitalTwinMetadata()
				{
					ModelId = calculatedPoint.ModelId,
					PropertyMetadata = new Dictionary<string, DigitalTwinPropertyMetadata>()
					{
						["TwinPropertyMetaData"] = new DigitalTwinPropertyMetadata() { LastUpdatedOn = calculatedPoint.LastUpdated }
					}
				},
				Contents = new Dictionary<string, object>()
				{
					["externalID"] = calculatedPoint.ExternalId,
					["connectorID"] = calculatedPoint.ConnectorID,
					["name"] = calculatedPoint.Name,
					["description"] = calculatedPoint.Description ?? calculatedPoint.Name,
					["type"] = calculatedPoint.Type.ToString().ToLowerInvariant(),
					["unit"] = calculatedPoint.Unit,
					["trendInterval"] = calculatedPoint.TrendInterval,
					["displayPriority"] = 1,
					["enabled"] = true, //Default - Rule instance Disabled is used to either upsert or delete
					["siteID"] = calculatedPoint.SiteId ?? null
				}
			};
#pragma warning restore CS8601 // Possible null reference assignment.

			return newTwin;
		}
	}
}

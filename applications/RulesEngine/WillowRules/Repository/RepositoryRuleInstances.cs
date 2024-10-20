using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.DTO;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Sources;
namespace Willow.Rules.Repository;

/// <summary>
/// A repository for <see cref="RuleInstance"/>s
/// </summary>
public interface IRepositoryRuleInstances : IRepositoryBase<RuleInstance>
{
	/// <summary>
	/// Get rule instances with metadata pre-joined
	/// </summary>
	Task<Batch<(RuleInstance ruleInstance, RuleInstanceMetadata metadata)>> GetAllCombined(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<RuleInstance, bool>>? whereExpression = null,
		int? page = null,
		int? take = null);

	/// <summary>
	/// Get rule instances with metadata pre-joined
	/// </summary>
	IAsyncEnumerable<(RuleInstance ruleInstance, RuleInstanceMetadata metadata)> GetAllCombinedAsync(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<RuleInstance, bool>>? whereExpression = null,
		int? page = null,
		int? take = null);

	/// <summary>
	/// Get rule instances that match an element in the location array in JSON
	/// </summary>
	Task<IEnumerable<RuleInstance>> GetByLocation(string location);

	/// <summary>
	/// Get rule instances that match an element in the fedby array in JSON
	/// </summary>
	Task<IEnumerable<RuleInstance>> GetFedBy(string twinId);

	/// <summary>
	/// Get rule instances that match an element in the feeds array in JSON
	/// </summary>
	Task<IEnumerable<RuleInstance>> GetFeeds(string twinId);

	/// <summary>
	/// Sets a rule instance to disabled or enabled
	/// </summary>
	/// <param name="ruleInstanceId"></param>
	/// <param name="disabled"></param>
	Task<int> SetDisabled(string ruleInstanceId, bool disabled);

	/// <summary>
	/// Delete rule instances where the LastUpdated value is earlier than the provided date
	/// </summary>
	Task<int> DeleteInstancesBefore(DateTimeOffset date, string ruleId, bool isCalculatedPointsOnly = false, CancellationToken cancellationToken = default);

	/// <summary>
	/// Delete rule instances by ruleId
	/// </summary>
	Task<int> DeleteInstancesByRuleId(string ruleId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a rule instance that is in a timezone other than UTC
	/// </summary>
	/// <returns></returns>
	Task<string> GetFirstNonUTCInstance();

	/// <summary>
	/// Gets minimal info to display a rule instance list
	/// </summary>
	/// <returns></returns>
	Task<IEnumerable<(string id, string equipmentId, string equipmentName, RuleInstanceStatus status)>> GetRuleInstanceList(string ruleId);

	/// <summary>
	/// Get all rules instances without expressions
	/// </summary>
	/// <returns></returns>
	IAsyncEnumerable<RuleInstance> GetRuleInstancesWithoutExpressions(Expression<Func<RuleInstance, bool>>? whereExpression = null);
}

/// <summary>
/// A repository for <see cref="RuleInstance"/>s
/// </summary>
public class RepositoryRuleInstances : RepositoryBase<RuleInstance>, IRepositoryRuleInstances
{
	/// <summary>
	/// Creates a new <see cref="RepositoryRuleInstance" />
	/// </summary>
	public RepositoryRuleInstances(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryRuleInstances> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.RuleInstances, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	protected override IQueryable<RuleInstance> ApplySort(IQueryable<RuleInstance> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<RuleInstance>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(RuleInstance.RuleId):
					{
						result = AddSort(queryable, result!, first, x => x.RuleId, sortSpecification.sort);
						break;
					}
				case nameof(RuleInstance.RuleName):
					{
						result = AddSort(queryable, result!, first, x => x.RuleName, sortSpecification.sort);
						break;
					}
				case nameof(RuleInstance.EquipmentId):
					{
						result = AddSort(queryable, result!, first, x => x.EquipmentId, sortSpecification.sort);
						break;
					}
				case nameof(RuleInstance.EquipmentName):
					{
						result = AddSort(queryable, result!, first, x => x.EquipmentName, sortSpecification.sort);
						break;
					}
				case nameof(RuleInstance.RuleDependencyCount):
					{
						result = AddSort(queryable, result!, first, x => x.RuleDependencyCount, sortSpecification.sort);
						break;
					}
				case nameof(RuleInstance.Status):
					{
						result = AddSort(queryable, result!, first, x => x.Status, sortSpecification.sort);
						result = AddSort(queryable, result!, false, x => x.Id, sortSpecification.sort);
						break;
					}
				case nameof(RuleInstance.CapabilityCount):
					{
						result = AddSort(queryable, result!, first, x => x.CapabilityCount, sortSpecification.sort);
						break;
					}
				case nameof(RuleInstance.Disabled):
					{
						// SQL can't sort by bool alone, so do by valid and then by id
						result = AddSort(queryable, result!, first, x => (x.Disabled || x.Status != RuleInstanceStatus.Valid) ? 0 : 1, sortSpecification.sort);
						result = AddSort(queryable, result!, false, x => x.Id, sortSpecification.sort);
						break;
					}
				case nameof(RuleInstance.TwinLocations):
					{
						result = AddSort(queryable, result!, first, x => (string)(object)x.TwinLocations, sortSpecification.sort);
						break;
					}
				default:
				case nameof(RuleInstance.Id):
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	protected override Expression<Func<RuleInstance, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(RuleInstance.Id):
				{
					return filter.CreateExpression((RuleInstance v) => v.Id, filter.ToString(formatProvider));
				}
			case "Enabled":
				{
					var disabled = !filter.ToBoolean(formatProvider);
					return filter.CreateExpression((RuleInstance v) => v.Disabled, disabled);
				}
			case nameof(RuleInstance.EquipmentUniqueId):
				{
					return filter.CreateExpression((RuleInstance v) => v.EquipmentUniqueId, filter.ToGuid(formatProvider));
				}
			case nameof(RuleInstance.EquipmentId):
				{
					return filter.CreateExpression((RuleInstance v) => v.EquipmentId, filter.ToString(formatProvider));
				}
			case nameof(RuleInstance.EquipmentName):
				{
					return filter.CreateExpression((RuleInstance v) => v.EquipmentName, filter.ToString(formatProvider));
				}
			case nameof(RuleInstance.RuleId):
				{
					return filter.CreateExpression((RuleInstance v) => v.RuleId, filter.ToString(formatProvider));
				}
			case nameof(RuleInstance.RuleName):
				{
					return filter.CreateExpression((RuleInstance v) => v.RuleName, filter.ToString(formatProvider));
				}
			case nameof(RuleInstance.Status):
				{
					return filter.CreateExpression((RuleInstance v) => (int)v.Status, (int)filter.ToEnum<RuleInstanceStatus>(formatProvider), isFlag: true);
				}
			case nameof(RuleInstance.Disabled):
				{
					return filter.CreateExpression((RuleInstance v) => v.Disabled, filter.ToBoolean(formatProvider));
				}
			case nameof(RuleInstance.RuleDependencyCount):
				{
					return filter.CreateExpression((RuleInstance v) => v.RuleDependencyCount, filter.ToInt32(formatProvider));
				}
			case nameof(RuleInstance.CapabilityCount):
				{
					return filter.CreateExpression((RuleInstance v) => v.CapabilityCount, filter.ToInt32(formatProvider));
				}
			case nameof(RuleInstance.TwinLocations):
				{
					return filter.CreateExpression((RuleInstance v) => (string)(object)v.TwinLocations, filter.ToString(formatProvider));
				}
			default:
				{
					return null;
				}
		}
	}

	/// <summary>
	/// Add the must-carry array fields to the query
	/// </summary>
	public override IQueryable<RuleInstance> WithArrays(IQueryable<RuleInstance> input)
	{
		return input
			//.Include(a => a.ParentRuleInstanceIds) - not currently in use
			// .Include(a => a.PointEntityIds)
			// .Include(a => a.RuleParametersBound)
			;
	}

	public async Task<int> SetDisabled(string ruleInstanceId, bool disabled)
	{
		var dummy = new RuleInstance() { Id = ruleInstanceId, Disabled = !disabled };
		dbSet.Attach(dummy).Property(x => x.Disabled).CurrentValue = disabled;
		this.InvalidateCache();
		return await rulesContext.SaveChangesAsync();
	}

	public IAsyncEnumerable<(RuleInstance ruleInstance, RuleInstanceMetadata metadata)> GetAllCombinedAsync(SortSpecificationDto[] sortSpecifications, FilterSpecificationDto[] filterSpecifications, Expression<Func<RuleInstance, bool>>? whereExpression = null, int? page = null, int? take = null)
	{
		var batch = BuildQueryable(sortSpecifications, filterSpecifications, whereExpression);

		return batch.ToAsyncEnumerable().Select(v => (v.ruleInstance, v.metadata));
	}

	/// <summary>
	/// Get rule instances and associated metadata in a single call
	/// </summary>
	public async Task<Batch<(RuleInstance ruleInstance, RuleInstanceMetadata metadata)>> GetAllCombined(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<RuleInstance, bool>>? whereExpression,
		int? page = null,
		int? take = null)
	{
		var batch = BuildQueryable(sortSpecifications, filterSpecifications, whereExpression);

		var result = await GetOrCreateBatch(batch, (v) => v.ruleInstance.Id, page, take);

		return result.Transform(v => (v.ruleInstance, v.metadata));
	}

	private IQueryable<RuleInstanceMetadataTuple> BuildQueryable(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<RuleInstance, bool>>? whereExpression)
	{
		whereExpression = BuildWhereClause(filterSpecifications, whereExpression);
		var queryable = whereExpression is null ? dbSet : dbSet.Where(whereExpression);

		queryable = WithArrays(queryable);

		var batch = (from t in queryable
					 join il in rulesContext.RuleInstanceMetadatas.AsNoTracking() on t.Id equals il.Id into j1
					 from j in j1.DefaultIfEmpty()
					 select new RuleInstanceMetadataTuple { ruleInstance = t, metadata = j });

		var expression = batch.AddFilter(filterSpecifications, nameof(RuleInstanceMetadata.ReviewStatus), (v) => v.metadata.ReviewStatus, (f) => f.ToEnum<ReviewStatus>(null));
		expression = batch.AddFilter(filterSpecifications, nameof(RuleInstanceMetadata.Tags), (v) => (string)(object)v.metadata.Tags, (f) => f.ToString(null), expression);
		expression = batch.AddFilter(filterSpecifications, nameof(RuleInstanceMetadata.TotalComments), (v) => v.metadata.TotalComments, (f) => f.ToInt32(null), expression);

		if (expression is not null)
		{
			batch = batch.Where(expression);
		}

		// Sort joined rule instance/metadata batch
		IOrderedQueryable<RuleInstanceMetadataTuple> ordered = null!;

		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(RuleInstance.RuleId):
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.ruleInstance.RuleId);
						break;
					}
				case nameof(RuleInstance.RuleName):
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.ruleInstance.RuleName);
						break;
					}
				case nameof(RuleInstance.EquipmentId):
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.ruleInstance.EquipmentId);
						break;
					}
				case nameof(RuleInstance.EquipmentName):
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.ruleInstance.EquipmentName);
						break;
					}
				case nameof(RuleInstance.RuleDependencyCount):
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.ruleInstance.RuleDependencyCount);
						break;
					}
				case nameof(RuleInstance.CapabilityCount):
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.ruleInstance.CapabilityCount);
						break;
					}
				case nameof(RuleInstance.Status):
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.ruleInstance.Status);
						break;
					}
				case nameof(RuleInstance.Disabled):
					{
						// SQL can't sort by bool alone, so do by valid and then by id
						ordered = batch.AddSort(ordered!, sortSpecification, f => (f.ruleInstance.Disabled || f.ruleInstance.Status != RuleInstanceStatus.Valid) ? 0 : 1);
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.ruleInstance.Id);
						break;
					}
				case nameof(RuleInstance.TwinLocations):
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.ruleInstance.TwinLocations);
						break;
					}
				case nameof(RuleInstanceMetadata.TriggerCount):
					{
						ordered = batch.AddSort(null!, sortSpecification, f => f.metadata.TriggerCount);
						break;
					}
				case nameof(RuleInstanceMetadata.ReviewStatus):
					{
						ordered = batch.AddSort(null!, sortSpecification, f => f.metadata.ReviewStatus);
						break;
					}
				case nameof(RuleInstanceMetadata.LastCommentPosted):
					{
						ordered = batch.AddSort(null!, sortSpecification, f => f.metadata.LastCommentPosted);
						break;
					}
				case nameof(RuleInstanceMetadata.TotalComments):
					{
						ordered = batch.AddSort(null!, sortSpecification, f => f.metadata.TotalComments);
						break;
					}
				default:
				case nameof(RuleInstance.Id):
					{
						ordered = batch.AddSort(ordered!, sortSpecification, f => f.ruleInstance.Id);
						break;
					}
			}
		}

		return ordered ?? batch;
	}

	/// <summary>
	/// A tuple containing a rule instance and its rule instance metadata. Represents a join between their queries.
	/// </summary>
	private class RuleInstanceMetadataTuple
	{
		public RuleInstance ruleInstance { get; set; } = null!;
		public RuleInstanceMetadata metadata { get; set; } = null!;
	}

	/// <summary>
	/// Get rule instances that match on location using a string search into the JSON
	/// </summary>
	/// <remarks>
	/// So, this is not ideal but having chosen to put the array in as JSON it seems to be the only way
	/// to get a query to work against it.
	/// </remarks>
	public async Task<IEnumerable<RuleInstance>> GetByLocation(string location)
	{
		location = $"%{location}%";
		// See https://docs.microsoft.com/en-us/ef/core/querying/raw-sql
		var items = await this.dbSet
			.FromSqlInterpolated<RuleInstance>($"SELECT * FROM [RuleInstance] WHERE [Locations] LIKE {location}")
			.ToListAsync();
		return items;
	}

	/// <summary>
	/// Get rule instances that match on feeds using a string search into the JSON
	/// </summary>
	/// <remarks>
	/// So, this is not ideal but having chosen to put the array in as JSON it seems to be the only way
	/// to get a query to work against it.
	/// </remarks>
	public async Task<IEnumerable<RuleInstance>> GetFeeds(string twinId)
	{
		twinId = $"%{twinId}%";
		// See https://docs.microsoft.com/en-us/ef/core/querying/raw-sql
		var items = await this.dbSet
			.FromSqlInterpolated<RuleInstance>($"SELECT * FROM [RuleInstance] WHERE [Feeds] LIKE {twinId}")
			.ToListAsync();
		return items;
	}

	/// <summary>
	/// Get rule instances that match on isFedBy using a string search into the JSON
	/// </summary>
	/// <remarks>
	/// So, this is not ideal but having chosen to put the array in as JSON it seems to be the only way
	/// to get a query to work against it.
	/// </remarks>
	public async Task<IEnumerable<RuleInstance>> GetFedBy(string twinId)
	{
		twinId = $"%{twinId}%";
		// See https://docs.microsoft.com/en-us/ef/core/querying/raw-sql
		var items = await this.dbSet
			.FromSqlInterpolated<RuleInstance>($"SELECT * FROM [RuleInstance] WHERE [FedBy] LIKE {twinId}")
			.ToListAsync();
		return items;
	}

	public async Task<int> DeleteInstancesBefore(DateTimeOffset date, string ruleId, bool isCalculatedPointsOnly = false, CancellationToken cancellationToken = default)
	{
		using (var timed = logger.TimeOperation(TimeSpan.FromSeconds(30), "Remove all rule instances for rule {ruleId} before {date}", ruleId, date))
		{
			int count = 0;

			try
			{
				await ExecuteAsync(async () =>
				{
					if (!string.IsNullOrEmpty(ruleId))
					{
						//only delete for rules where the scan completed successfully
						count = await this.rulesContext.Database
								.ExecuteSqlInterpolatedAsync(@$"
									SET NOCOUNT OFF; 
									DELETE ri FROM [RuleInstance] ri
									INNER JOIN [RuleMetadata] rm on rm.Id = ri.RuleId
									WHERE ri.RuleId={ruleId} AND ri.[LastUpdated] < {date} AND rm.ScanComplete = 1
									select @@ROWCOUNT");

						InvalidateOne(ruleId);
						//instances = await Get(v => v.LastUpdated < date && v.RuleId == ruleId);
					}
					else
					{
						var currentCount = 0;
						var batchSize = 100;

						do
						{
							//only delete for rules where the scan completed successfully
							if (isCalculatedPointsOnly)
							{
								currentCount = await this.rulesContext.Database
									.ExecuteSqlInterpolatedAsync(@$"
									SET NOCOUNT OFF; 
									DELETE TOP ({batchSize}) ri FROM [RuleInstance] ri
									INNER JOIN [RuleMetadata] rm on rm.Id = ri.RuleId
									WHERE ri.[LastUpdated] < {date} AND rm.ScanComplete = 1 AND ri.[RuleTemplate] = {RuleTemplateCalculatedPoint.ID}
									select @@ROWCOUNT");
							}
							else
							{
								currentCount = await this.rulesContext.Database
									.ExecuteSqlInterpolatedAsync(@$"
									SET NOCOUNT OFF; 
									DELETE TOP ({batchSize}) ri FROM [RuleInstance] ri
									INNER JOIN [RuleMetadata] rm on rm.Id = ri.RuleId
									WHERE ri.[LastUpdated] < {date} AND rm.ScanComplete = 1
									select @@ROWCOUNT");
							}

							count += currentCount;

							logger.LogDebug("{count} instances deleted so far {name} < {date}", count, nameof(RuleInstance.LastUpdated), date);
						}
						while (currentCount > 0);

						this.InvalidateCache();
						//instances = await Get(v => v.LastUpdated < date);
					}

					logger.LogInformation("Deleted {count} old rule instances {name} < {date}", count, nameof(RuleInstance.LastUpdated), date);

					return count;
				});
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to RemoveAllRuleInstancesForRule");
				return 0;
			}

			return count;
		}
	}

	/// <summary>
	/// Delete rule instances by ruleId
	/// </summary>
	public async Task<int> DeleteInstancesByRuleId(string ruleId, CancellationToken cancellationToken = default)
	{
		using (var timed = logger.TimeOperation(TimeSpan.FromSeconds(30), "Delete all rule instances for rule {ruleId}", ruleId))
		{
			int count = 0;

			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				await ExecuteAsync(async () =>
				{
					var currentCount = 0;
					var batchSize = 100;

					do
					{
						currentCount = await this.rulesContext.Database
							.ExecuteSqlInterpolatedAsync($"SET NOCOUNT OFF; DELETE TOP ({batchSize}) FROM [RuleInstance] WHERE [RuleInstance].[RuleId]={ruleId} select @@ROWCOUNT");

						count += currentCount;

						logger.LogDebug("{count} instances deleted so far {ruleId}", count, ruleId);
					}
					while (currentCount > 0);

					this.InvalidateCache();

					logger.LogInformation("Deleted {count} rule instances {ruleId}", count, ruleId);
				});
			}
			catch (OperationCanceledException ex)
			{
				logger.LogError(ex, "Delete {ruleId} instances cancelled", ruleId);
				throw;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to delete instances for rule {ruleId}", ruleId);
				throw;
			}

			return count;
		}
	}

	public async Task<string> GetFirstNonUTCInstance()
	{
		var items = (from p in this.dbSet
					 select new
					 {
						 timeZone = p.TimeZone
					 });

		await foreach (var item in items.ToAsyncEnumerable())
		{
			if (item.timeZone != "UTC")
			{
				return item.timeZone;
			}
		}

		return string.Empty;
	}

	public async Task<IEnumerable<(string id, string equipmentId, string equipmentName, RuleInstanceStatus status)>> GetRuleInstanceList(string ruleId)
	{
		var items = await (from p in this.dbSet
						   where p.RuleId == ruleId
						   select new
						   {
							   id = p.Id,
							   equipmentId = p.EquipmentId,
							   equipmentName = p.EquipmentName,
							   status = p.Status
						   })
				.ToListAsync();

		return items.Select(v => (v.id, v.equipmentId, v.equipmentName, v.status));
	}

	public IAsyncEnumerable<RuleInstance> GetRuleInstancesWithoutExpressions(Expression<Func<RuleInstance, bool>>? whereExpression = null)
	{
		IQueryable<RuleInstance> queryable = this.dbSet;

		if(whereExpression is not null)
		{
			queryable = queryable.Where(whereExpression);
		}

		var items = (from v in queryable
					 select new
						{
							v.Id,
							v.EquipmentId,
							v.RuleName,
							v.RuleId,
							v.TwinLocations,
							v.FedBy,
							v.Feeds,
							v.PrimaryModelId,
							v.RuleCategory
						});

		return items
			.ToAsyncEnumerable()
			.Select(v => new RuleInstance()
			{
				Id = v.Id,
				EquipmentId = v.EquipmentId,
				RuleName = v.RuleName,
				RuleId = v.RuleId,
				TwinLocations = v.TwinLocations,
				FedBy = v.FedBy,
				Feeds = v.Feeds,
				PrimaryModelId = v.PrimaryModelId,
				RuleCategory = v.RuleCategory
			});
	}
}

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Rules.Configuration;
using Willow.Rules.DTO;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Repository;

public interface IRepositoryRuleInstanceMetadata : IRepositoryBase<RuleInstanceMetadata>
{
	/// <summary>
	/// Delete rule instance metadata by rule id
	/// </summary>
	Task<int> DeleteMetadataByRuleId(string ruleId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Delete rule instance metadata with no associated rule instance
	/// </summary>
	Task<int> DeleteOrphanMetadata(CancellationToken cancellationToken = default);

	/// <summary>
	/// Get or add metadata
	/// </summary>
	Task<RuleInstanceMetadata> GetOrAdd(string ruleInstanceId);
}

/// <summary>
/// A repository for <see cref="RuleInstanceMetadata"/>s
/// </summary>
public class RepositoryRuleInstanceMetadata : RepositoryBase<RuleInstanceMetadata>, IRepositoryRuleInstanceMetadata
{
	public RepositoryRuleInstanceMetadata(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryRuleInstanceMetadata> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.RuleInstanceMetadatas, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	/// <inheritdoc />
	protected override IQueryable<RuleInstanceMetadata> ApplySort(IQueryable<RuleInstanceMetadata> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<RuleInstanceMetadata>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(RuleInstanceMetadata.LastTriggered):
					{
						result = AddSort(queryable, result!, first, x => x.LastTriggered, sortSpecification.sort);
						break;
					}
				case nameof(RuleInstanceMetadata.TriggerCount):
					{
						result = AddSort(queryable, result!, first, x => x.TriggerCount, sortSpecification.sort);
						break;
					}
				default:
				case nameof(RuleInstanceMetadata.Id):
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	/// <summary>
	/// Add the must-carry array fields to the query
	/// </summary>
	public override IQueryable<RuleInstanceMetadata> WithArrays(IQueryable<RuleInstanceMetadata> input)
	{
		return input;
	}

	/// <inheritdoc />
	public async Task<RuleInstanceMetadata> GetOrAdd(string ruleInstanceId)
	{
		var existing = await this.GetOne(ruleInstanceId, updateCache: false);

		if (existing is null)
		{
			existing = new RuleInstanceMetadata()
			{
				Id = ruleInstanceId
			};
		}

		return existing;
	}

	protected override Expression<Func<RuleInstanceMetadata, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(RuleInstanceMetadata.Id):
				{
					return filter.CreateExpression((RuleInstanceMetadata v) => v.Id, filter.ToString(formatProvider));
				}
			default:
				{
					return null;
				}
		}
	}

	/// <summary>
	/// Delete rule instance metadata by rule id
	/// </summary>
	public async Task<int> DeleteMetadataByRuleId(string ruleId, CancellationToken cancellationToken = default)
	{
		using (var timed = logger.TimeOperation(TimeSpan.FromSeconds(30), "Delete all metadata for rule {ruleId}", ruleId))
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
							.ExecuteSqlInterpolatedAsync($"SET NOCOUNT OFF; DELETE TOP ({batchSize}) rim FROM [RuleInstanceMetadata] rim INNER JOIN [RuleInstance] ri ON ri.Id = rim.Id WHERE ri.RuleId = {ruleId} select @@ROWCOUNT");

						count += currentCount;

						logger.LogDebug("{count} instance metadata deleted so far {ruleId}", count, ruleId);
					}
					while (currentCount > 0);

					this.InvalidateCache();

					logger.LogInformation("Deleted {count} rule instance metadata {ruleId}", count, ruleId);
				});
			}
			catch (OperationCanceledException ex)
			{
				logger.LogError(ex, "Delete {ruleId} instance metadata cancelled", ruleId);
				throw;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to delete metadata for rule instance {ruleId}", ruleId);
				throw;
			}

			return count;
		}
	}

	/// <summary>
	/// Delete metadata with no associated rule instance
	/// </summary>
	/// <remarks>
	/// Deleting a rule while batch or realtime rule execution is processing can cause metadata with no rule
	/// </remarks>
	public async Task<int> DeleteOrphanMetadata(CancellationToken cancellationToken = default)
	{
		using (var timed = logger.TimeOperation(TimeSpan.FromSeconds(30), "Delete all metadata with no rule instance"))
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
							.ExecuteSqlInterpolatedAsync($"SET NOCOUNT OFF; DELETE TOP ({batchSize}) rim FROM [RuleInstanceMetadata] rim LEFT JOIN [RuleInstance] ri ON rim.Id = ri.Id WHERE ri.Id IS NULL select @@ROWCOUNT");

						count += currentCount;

						logger.LogDebug("{count} metadata with no rule instance deleted so far", count);
					}
					while (currentCount > 0);

					this.InvalidateCache();

					logger.LogInformation("Deleted {count} metadata with no rule instance", count);
				});
			}
			catch (OperationCanceledException ex)
			{
				logger.LogError(ex, "Delete metadata with no rule instance cancelled");
				throw;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to delete metadata with no rule instance");
				throw;
			}

			return count;
		}
	}
}

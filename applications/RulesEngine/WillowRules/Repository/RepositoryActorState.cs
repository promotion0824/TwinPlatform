using System;
using System.Collections.Generic;
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

/// <summary>
/// Repository for <see cref="ActorState" />
/// </summary>
/// <remarks>
/// This is now backed by a disk cache not the database, it relies on there being a persistent volume
/// connected to the Rules Engine processor.
/// </remarks>
public interface IRepositoryActorState : IRepositoryBase<ActorState>
{
	/// <summary>
	/// Get all actors
	/// </summary>
	IAsyncEnumerable<ActorState> GetAllActors();

	/// <summary>
	/// Delete all actors from DB
	/// </summary>
	Task RemoveAll();

	/// <summary>
	/// Delete actors by rule id
	/// </summary>
	Task<int> DeleteActorsByRuleId(string ruleId, CancellationToken cancellationToken);

	/// <summary>
	/// Delete actors with no associated rule instance
	/// </summary>
	Task<int> DeleteOrphanActors(CancellationToken cancellationToken = default);
}

/// <summary>
/// A repository for <see cref="ActorState"/>s
/// </summary>
public class RepositoryActorState : RepositoryBase<ActorState>, IRepositoryActorState
{
	/// <summary>
	/// Creates a new repository for <see cref="ActorState" />
	/// </summary>
	public RepositoryActorState(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryActorState> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.ActorState, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	protected override Expression<Func<ActorState, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(ActorState.Id):
				{
					return filter.CreateExpression((ActorState v) => v.Id, filter.ToString(formatProvider));
				}
			default:
				{
					return null;
				}
		}
	}

	/// <summary>
	/// GetAll for ActorState uses a disk cache not cosmos and doesn't support sorting or filtering
	/// </summary>
	public IAsyncEnumerable<ActorState> GetAllActors()
	{
		return GetAll();
	}

	public override IQueryable<ActorState> WithArrays(IQueryable<ActorState> input)
	{
		return input;
	}

	public async Task RemoveAll()
	{
		logger.LogInformation($"Remove all actors");

		await ExecuteAsync(async () =>
		{
			await this.rulesContext.Database.ExecuteSqlInterpolatedAsync($"TRUNCATE TABLE [Actors]");
		});
	}

	protected override IQueryable<ActorState> ApplySort(IQueryable<ActorState> queryable, SortSpecificationDto[] sortSpecifications)
	{
		IOrderedQueryable<ActorState>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				default:
				case nameof(RuleMetadata.Id):
					{
						result = queryable.OrderBy(x => x.Id);
						break;
					}
			}
		}
		return queryable;
	}

	/// <summary>
	/// Delete actors by rule id
	/// </summary>
	public async Task<int> DeleteActorsByRuleId(string ruleId, CancellationToken cancellationToken)
	{
		using (var timed = logger.TimeOperation(TimeSpan.FromSeconds(30), "Delete all actors for rule {ruleId}", ruleId))
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
							.ExecuteSqlInterpolatedAsync($"SET NOCOUNT OFF; DELETE TOP ({batchSize}) FROM [Actors] WHERE [Actors].[RuleId]={ruleId} select @@ROWCOUNT");

						count += currentCount;

						logger.LogDebug("{count} actors deleted so far {name}", count, ruleId);
					}
					while (currentCount > 0);

					this.InvalidateCache();

					logger.LogInformation("Deleted {count} actors for {ruleId}", count, ruleId);
				});
			}
			catch (OperationCanceledException ex)
			{
				logger.LogError(ex, "Delete {ruleId} actors cancelled", ruleId);
				throw;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to delete actors for rule {ruleId}", ruleId);
				throw;
			}

			return count;
		}
	}

	/// <summary>
	/// Delete actors with no associated rule instance
	/// </summary>
	/// <remarks>
	/// Deleting a rule while batch or realtime rule execution is processing can cause actors with no rule instance
	/// </remarks>
	public async Task<int> DeleteOrphanActors(CancellationToken cancellationToken = default)
	{
		using (var timed = logger.TimeOperation(TimeSpan.FromSeconds(30), "Delete all actors with no rule instance"))
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
							.ExecuteSqlInterpolatedAsync($"SET NOCOUNT OFF; DELETE TOP ({batchSize}) act FROM [Actors] act LEFT JOIN [RuleInstance] ri ON act.Id = ri.Id WHERE ri.Id IS NULL select @@ROWCOUNT");

						count += currentCount;

						logger.LogDebug("{count} actors with no rule instance deleted so far", count);
					}
					while (currentCount > 0);

					this.InvalidateCache();

					logger.LogInformation("Deleted {count} actors with no rule instance", count);
				});
			}
			catch (OperationCanceledException ex)
			{
				logger.LogError(ex, "Delete actors with no rule instance cancelled");
				throw;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to delete actors with no rule instance");
				throw;
			}

			return count;
		}
	}
}

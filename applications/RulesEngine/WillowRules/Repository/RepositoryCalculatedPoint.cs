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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Willow.Rules.Repository;

public interface IRepositoryCalculatedPoint : IRepositoryBase<CalculatedPoint>
{
	/// <summary>
	/// Get calculated points with instance and metadata
	/// </summary>
	Task<Batch<(CalculatedPoint calculatedPoint, RuleInstance instance, RuleInstanceMetadata metadata, ActorState actor)>> GetAllCombined(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<CalculatedPoint, bool>>? whereExpression = null,
		int? page = null,
		int? take = null);

	/// <summary>
	/// Get a calculated point with instance and metadata
	/// </summary>
	Task<(CalculatedPoint calculatedPoint, RuleInstance? instance, RuleInstanceMetadata? metadata, ActorState? actor)?> GetOneCombined(string id);

	/// <summary>
	///	Clean out old calculated points
	/// </summary>
	Task<int> DeleteBefore(DateTimeOffset date, CalculatedPointSource source = 0, CancellationToken cancellationToken = default);

	/// <summary>
	/// Delete calculated points by rule id
	/// </summary>
	Task<int> ScheduleDeleteCalculatedPointsByRuleId(string ruleId, CancellationToken cancellationToken);
}

/// <summary>
/// A repository for <see cref="CalculatedPoint"/>s
/// </summary>
public class RepositoryCalculatedPoint : RepositoryBase<CalculatedPoint>, IRepositoryCalculatedPoint
{
	private readonly IRepositoryRuleInstances repositoryRuleInstances;
	private readonly IRepositoryRuleInstanceMetadata repositoryRuleInstanceMetadata;
	private readonly IRepositoryActorState repositoryActorState;

	public RepositoryCalculatedPoint(
		IDbContextFactory<RulesContext> dbContextFactory,
		RulesContext rulesContext,
		WillowEnvironmentId willowEnvironment,
		IMemoryCache memoryCache,
		IEpochTracker epochTracker,
		IRepositoryRuleInstances repositoryRuleInstances,
		IRepositoryRuleInstanceMetadata repositoryRuleInstanceMetadata,
		IRepositoryActorState repositoryActorState,
		ILogger<RepositoryCalculatedPoint> logger,
		IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.CalculatedPoints, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
		this.repositoryRuleInstances = repositoryRuleInstances ?? throw new ArgumentNullException(nameof(repositoryRuleInstances));
		this.repositoryRuleInstanceMetadata = repositoryRuleInstanceMetadata ?? throw new ArgumentNullException(nameof(repositoryRuleInstanceMetadata));
		this.repositoryActorState = repositoryActorState ?? throw new ArgumentNullException(nameof(repositoryActorState));
	}

	protected override Expression<Func<CalculatedPoint, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(CalculatedPoint.Id):
				{
					return filter.CreateExpression((CalculatedPoint v) => v.Id, filter.ToString(formatProvider));
				}
			case nameof(CalculatedPoint.Name):
				{
					return filter.CreateExpression((CalculatedPoint v) => v.Name, filter.ToString(formatProvider));
				}
			case nameof(CalculatedPoint.ModelId):
				{
					return filter.CreateExpression((CalculatedPoint v) => v.ModelId, filter.ToString(formatProvider));
				}
			case nameof(CalculatedPoint.IsCapabilityOf):
				{
					return filter.CreateExpression((CalculatedPoint v) => v.IsCapabilityOf, filter.ToString(formatProvider));
				}
			case nameof(CalculatedPoint.Unit):
				{
					return filter.CreateExpression((CalculatedPoint v) => v.Unit, filter.ToString(formatProvider));
				}
			case nameof(CalculatedPoint.TimeZone):
				{
					return filter.CreateExpression((CalculatedPoint v) => v.TimeZone, filter.ToString(formatProvider));
				}
			case nameof(CalculatedPoint.Source):
				{
					return filter.CreateExpression((CalculatedPoint v) => v.Source, filter.ToEnum<CalculatedPointSource>(formatProvider));
				}
			case nameof(CalculatedPoint.ActionRequired):
				{
					return filter.CreateExpression((CalculatedPoint v) => v.ActionRequired, filter.ToEnum<ADTActionRequired>(formatProvider));
				}
			case nameof(CalculatedPoint.ActionStatus):
				{
					return filter.CreateExpression((CalculatedPoint v) => v.ActionStatus, filter.ToEnum<ADTActionStatus>(formatProvider));
				}
			default:
				{
					return null;
				}
		}
	}

	/// <summary>
	/// Get calculated point, instance and metadata in a single call
	/// </summary>
	public async Task<Batch<(CalculatedPoint calculatedPoint, RuleInstance instance, RuleInstanceMetadata metadata, ActorState actor)>> GetAllCombined(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<CalculatedPoint, bool>>? whereExpression,
		int? page = null,
		int? take = null)
	{
		whereExpression = BuildWhereClause(filterSpecifications, whereExpression);
		var queryable = whereExpression is null ? dbSet : dbSet.Where(whereExpression);

		queryable = ApplyBatchParams(queryable, sortSpecifications);

		//good examples here
		//https://www.tektutorialshub.com/entity-framework-core/join-query-in-ef-core/#linq-join-on-multiple-columns

		var batch = from cp in queryable
					join ri in rulesContext.RuleInstances on cp.Id equals ri.Id into r1_group
					from r1_result in r1_group.DefaultIfEmpty()
					join m in rulesContext.RuleInstanceMetadatas on cp.Id equals m.Id into m_group
					from m_result in m_group.DefaultIfEmpty()
					join a in rulesContext.ActorState on cp.Id equals a.Id into a_group
					from a_result in a_group.DefaultIfEmpty()
					select new
					{
						point = cp,
						ruleInstance = r1_result,
						actor = a_result,
						metadata = m_result
					};

		var result = await GetOrCreateBatch(batch, (v) => v.point.Id, page, take);

		return result.Transform(v => (v.point, v.ruleInstance, v.metadata, v.actor));
	}

	public async Task<(CalculatedPoint calculatedPoint, RuleInstance? instance, RuleInstanceMetadata? metadata, ActorState? actor)?> GetOneCombined(string id)
	{
		var calculatedPoint = await this.GetOne(id);
		if (calculatedPoint is null) return null!;

		var instance = await this.repositoryRuleInstances.GetOne(id);

		var metadata = await this.repositoryRuleInstanceMetadata.GetOne(id);

		var actor = await this.repositoryActorState.GetOne(id);

		return (calculatedPoint, instance, metadata, actor);
	}

	/// <summary>
	/// Add the must-carry array fields to the query
	/// </summary>
	public override IQueryable<CalculatedPoint> WithArrays(IQueryable<CalculatedPoint> input)
	{
		return input;
	}

	/// <inheritdoc />
	protected override IQueryable<CalculatedPoint> ApplySort(IQueryable<CalculatedPoint> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<CalculatedPoint>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(CalculatedPoint.Name): // Name
					{
						result = AddSort(queryable, result!, first, x => x.Name, sortSpecification.sort);
						break;
					}
				case nameof(CalculatedPoint.ValueExpression): // Expression
					{
						result = AddSort(queryable, result!, first, x => x.ValueExpression, sortSpecification.sort);
						break;
					}
				case nameof(CalculatedPoint.ModelId):
					{
						result = AddSort(queryable, result!, first, x => x.ModelId, sortSpecification.sort);
						break;
					}
				case nameof(CalculatedPoint.IsCapabilityOf):
					{
						result = AddSort(queryable, result!, first, x => x.IsCapabilityOf, sortSpecification.sort);
						break;
					}
				case nameof(CalculatedPoint.Unit): // (not in table)
					{
						result = AddSort(queryable, result!, first, x => x.Unit, sortSpecification.sort);
						break;
					}
				case nameof(CalculatedPoint.TimeZone):
					{
						result = AddSort(queryable, result!, first, x => x.TimeZone, sortSpecification.sort);
						break;
					}
				case nameof(CalculatedPoint.Source):
					{
						result = AddSort(queryable, result!, first, x => x.Source, sortSpecification.sort);
						break;
					}
				case nameof(CalculatedPoint.ActionRequired):
					{
						result = AddSort(queryable, result!, first, x => x.ActionRequired, sortSpecification.sort);
						break;
					}
				case nameof(CalculatedPoint.ActionStatus):
					{
						result = AddSort(queryable, result!, first, x => x.ActionStatus, sortSpecification.sort);
						break;
					}
				case nameof(CalculatedPoint.LastSyncDateUTC): // (not in table)
					{
						result = AddSort(queryable, result!, first, x => x.LastSyncDateUTC, sortSpecification.sort);
						break;
					}
				default:
				case nameof(CalculatedPoint.Id): // Default to id
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	public async Task<int> DeleteBefore(DateTimeOffset date, CalculatedPointSource source = 0, CancellationToken cancellationToken = default)
	{
		using (var timed = logger.TimeOperation(TimeSpan.FromSeconds(30), "Remove all calculated points before {date}", date))
		{
			int count = 0;

			try
			{
				await ExecuteAsync(async () =>
				{
					var currentCount = 0;
					var batchSize = 100;

					do
					{
						//only delete for rules where the scan completed successfully
						currentCount = await this.rulesContext.Database
							.ExecuteSqlInterpolatedAsync(@$"
									SET NOCOUNT OFF; 
									DELETE TOP ({batchSize}) cp FROM [CalculatedPoints] cp
									WHERE cp.[LastUpdated] < {date} AND cp.[Source] = {source}
									select @@ROWCOUNT");

						count += currentCount;

						logger.LogDebug("{count} calc points deleted so far {name} < {date}", count, nameof(CalculatedPoint.LastUpdated), date);
					}
					while (currentCount > 0);

					this.InvalidateCache();

					logger.LogInformation("Deleted {count} old calc points {name} < {date}", count, nameof(CalculatedPoint.LastUpdated), date);

					return count;
				});
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to DeleteBefore");
				return 0;
			}

			return count;
		}
	}

	public async Task<int> ScheduleDeleteCalculatedPointsByRuleId(string ruleId, CancellationToken cancellationToken)
	{
		using var timed = logger.TimeOperation(TimeSpan.FromSeconds(30), "Schedule calculated points for deletion for {ruleId}", ruleId);
		int count = 0;

		try
		{
			count = await rulesContext.Database
				.ExecuteSqlInterpolatedAsync(
					@$"SET NOCOUNT OFF; 
					UPDATE [CalculatedPoints] SET [ActionRequired] = {ADTActionRequired.Delete}
					WHERE [RuleId] = {ruleId} AND [Source] = {CalculatedPointSource.RulesEngine}
					select @@ROWCOUNT", cancellationToken: cancellationToken);

			this.InvalidateCache();
			logger.LogInformation("{count} calculated points scheduled for deletion for {ruleId}", count, ruleId);

			return count;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to DeleteCalculatedPointsByRuleId for {ruleId}", ruleId);
			return 0;
		}
	}
}

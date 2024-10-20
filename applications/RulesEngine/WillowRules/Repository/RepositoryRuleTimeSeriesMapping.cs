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

/// <summary>
/// A repository for <see cref="RuleTimeSeriesMapping"/>s
/// </summary>
public interface IRepositoryRuleTimeSeriesMapping : IRepositoryBase<RuleTimeSeriesMapping>
{
	/// <summary>
	///	Clean out old time series mappings
	/// </summary>
	Task<int> DeleteBefore(DateTimeOffset date, string ruleId, CancellationToken cancellationToken = default);
}

/// <summary>
/// A repository for <see cref="RuleTimeSeriesMapping"/>s
/// </summary>
public class RepositoryRuleTimeSeriesMapping : RepositoryBase<RuleTimeSeriesMapping>, IRepositoryRuleTimeSeriesMapping
{
	public RepositoryRuleTimeSeriesMapping(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryRuleTimeSeriesMapping> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.RuleTimeSeriesMapping, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	public override IQueryable<RuleTimeSeriesMapping> WithArrays(IQueryable<RuleTimeSeriesMapping> input)
	{
		return input;
	}

	protected override Expression<Func<RuleTimeSeriesMapping, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(RuleTimeSeriesMapping.Id):
				{
					return filter.CreateExpression((RuleTimeSeriesMapping v) => v.Id, filter.ToString(formatProvider));
				}
			case nameof(RuleTimeSeriesMapping.DtId):
				{
					return filter.CreateExpression((RuleTimeSeriesMapping v) => v.DtId, filter.ToString(formatProvider));
				}
			case nameof(RuleTimeSeriesMapping.ExternalId):
				{
					return filter.CreateExpression((RuleTimeSeriesMapping v) => v.ExternalId, filter.ToString(formatProvider));
				}
			case nameof(RuleTimeSeriesMapping.TrendId):
				{
					return filter.CreateExpression((RuleTimeSeriesMapping v) => v.TrendId, filter.ToString(formatProvider));
				}
			case nameof(RuleTimeSeriesMapping.RuleId):
				{
					return filter.CreateExpression((RuleTimeSeriesMapping v) => v.RuleId, filter.ToString(formatProvider));
				}
			default:
				{
					return null;
				}
		}
	}

	protected override IQueryable<RuleTimeSeriesMapping> ApplySort(IQueryable<RuleTimeSeriesMapping> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<RuleTimeSeriesMapping>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(RuleTimeSeriesMapping.DtId):
					{
						result = AddSort(queryable, result!, first, x => x.DtId, sortSpecification.sort);
						break;
					}
				case nameof(RuleTimeSeriesMapping.ExternalId):
					{
						result = AddSort(queryable, result!, first, x => x.ExternalId, sortSpecification.sort);
						break;
					}
				case nameof(RuleTimeSeriesMapping.TrendId):
					{
						result = AddSort(queryable, result!, first, x => x.TrendId, sortSpecification.sort);
						break;
					}
				default:
				case nameof(RuleTimeSeriesMapping.RuleId):
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	public async Task<int> DeleteBefore(DateTimeOffset date, string ruleId, CancellationToken cancellationToken = default)
	{
		var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));

		using (var timed = logger.TimeOperation(TimeSpan.FromSeconds(30), "Remove rule time series mappings before {date}", date))
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
						currentCount = await this.rulesContext.Database
							.ExecuteSqlInterpolatedAsync(@$"
								SET NOCOUNT OFF;
								DELETE TOP ({batchSize}) ri FROM [RuleTimeSeriesMapping] ri
								WHERE ({ruleId} is null or {ruleId} = '' or ri.RuleId={ruleId}) and ri.[LastUpdate] < {date}
								select @@ROWCOUNT");

						count += currentCount;

						throttledLogger.LogInformation("{count} instances deleted so far {name} < {date}", count, nameof(RuleTimeSeriesMapping.LastUpdate), date);
					}
					while (currentCount > 0);

					this.InvalidateCache();
					//instances = await Get(v => v.LastUpdated < date);


					logger.LogInformation("Deleted {count} old rule time series mappings {name} < {date}", count, nameof(RuleTimeSeriesMapping.LastUpdate), date);

					return count;
				});
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to Remove  rule old time series mappings");
				return 0;
			}

			return count;
		}
	}
}

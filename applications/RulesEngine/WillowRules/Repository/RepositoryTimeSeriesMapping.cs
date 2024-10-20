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
/// A repository for <see cref="TimeSeriesMapping"/>s
/// </summary>
public interface IRepositoryTimeSeriesMapping : IRepositoryBase<TimeSeriesMapping>
{
	/// <summary>
	///	Clean out old time series mappings
	/// </summary>
	Task<int> DeleteBefore(DateTimeOffset date, CancellationToken cancellationToken = default);
}

/// <summary>
/// A repository for <see cref="TimeSeriesMapping"/>s
/// </summary>
public class RepositoryTimeSeriesMapping : RepositoryBase<TimeSeriesMapping>, IRepositoryTimeSeriesMapping
{
	public RepositoryTimeSeriesMapping(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryTimeSeriesMapping> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.TimeSeriesMappings, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	public override IQueryable<TimeSeriesMapping> WithArrays(IQueryable<TimeSeriesMapping> input)
	{
		return input;
	}

	protected override Expression<Func<TimeSeriesMapping, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(TimeSeriesMapping.Id):
				{
					return filter.CreateExpression((TimeSeriesMapping v) => v.Id, filter.ToString(formatProvider));
				}
			case nameof(TimeSeriesMapping.DtId):
				{
					return filter.CreateExpression((TimeSeriesMapping v) => v.DtId, filter.ToString(formatProvider));
				}
			case nameof(TimeSeriesMapping.ModelId):
				{
					return filter.CreateExpression((TimeSeriesMapping v) => v.ModelId, filter.ToString(formatProvider));
				}
			case nameof(TimeSeriesMapping.Unit):
				{
					return filter.CreateExpression((TimeSeriesMapping v) => v.Unit, filter.ToString(formatProvider));
				}
			case nameof(TimeSeriesMapping.ConnectorId):
				{
					return filter.CreateExpression((TimeSeriesMapping v) => v.ConnectorId, filter.ToString(formatProvider));
				}
			case nameof(TimeSeriesMapping.ExternalId):
				{
					return filter.CreateExpression((TimeSeriesMapping v) => v.ExternalId, filter.ToString(formatProvider));
				}
			case nameof(TimeSeriesMapping.TrendId):
				{
					return filter.CreateExpression((TimeSeriesMapping v) => v.TrendId, filter.ToString(formatProvider));
				}
			default:
				{
					return null;
				}
		}
	}

	protected override IQueryable<TimeSeriesMapping> ApplySort(IQueryable<TimeSeriesMapping> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<TimeSeriesMapping>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(TimeSeriesMapping.DtId):
					{
						result = AddSort(queryable, result!, first, x => x.DtId, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeriesMapping.ConnectorId):
					{
						result = AddSort(queryable, result!, first, x => x.ConnectorId, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeriesMapping.ExternalId):
					{
						result = AddSort(queryable, result!, first, x => x.ExternalId, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeriesMapping.TrendId):
					{
						result = AddSort(queryable, result!, first, x => x.TrendId, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeriesMapping.TrendInterval):
					{
						result = AddSort(queryable, result!, first, x => x.TrendInterval, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeriesMapping.ModelId):
					{
						result = AddSort(queryable, result!, first, x => x.ModelId, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeriesMapping.Unit):
					{
						result = AddSort(queryable, result!, first, x => x.Unit, sortSpecification.sort);
						break;
					}
				default:
				case nameof(TimeSeries.Id):
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	public async Task<int> DeleteBefore(DateTimeOffset date, CancellationToken cancellationToken = default)
	{
		using (var timed = logger.TimeOperation(TimeSpan.FromSeconds(30), "Remove time series mappings before {date}", date))
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
								DELETE TOP ({batchSize}) ri FROM [TimeSeriesMapping] ri
								WHERE ri.[LastUpdate] < {date}
								select @@ROWCOUNT");

						count += currentCount;

						logger.LogDebug("{count} instances deleted so far {name} < {date}", count, nameof(TimeSeriesMapping.LastUpdate), date);
					}
					while (currentCount > 0);

					this.InvalidateCache();
					//instances = await Get(v => v.LastUpdated < date);


					logger.LogInformation("Deleted {count} old time series mappings {name} < {date}", count, nameof(TimeSeriesMapping.LastUpdate), date);

					return count;
				});
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to Remove old time series mappings");
				return 0;
			}

			return count;
		}
	}
}

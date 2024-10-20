using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Repository;

/// <summary>
/// A repository for <see cref="TimeSeriesBuffer"/>s
/// </summary>
public interface IRepositoryTimeSeriesBuffer : IRepositoryBase<TimeSeries>
{
	/// <summary>
	/// Delete all timeseries from DB
	/// </summary>
	Task RemoveAll();

	/// <summary>
	/// Gets a time series by twin id
	/// </summary>
	Task<TimeSeries?> GetByTwinId(string twinId);
}

/// <summary>
/// A repository for <see cref="TimeSeriesBuffer"/>s
/// </summary>
public class RepositoryTimeSeriesBuffer : RepositoryBase<TimeSeries>, IRepositoryTimeSeriesBuffer
{
	public RepositoryTimeSeriesBuffer(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryTimeSeriesBuffer> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.TimeSeriesBuffer, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	public override IQueryable<TimeSeries> WithArrays(IQueryable<TimeSeries> input)
	{
		return input;
	}

	/// <summary>
	/// Expression used to calculate score from boolean fields
	/// </summary>
	private Expression<Func<TimeSeries, int>> score = (TimeSeries x) =>
		(string.IsNullOrEmpty(x.DtId) ? (int)TimeSeriesStatus.NoTwin : 0) +
		(x.IsOffline ? (int)TimeSeriesStatus.Offline : 0) +
		(x.IsPeriodOutOfRange ? (int)TimeSeriesStatus.PeriodOutOfRange : 0) +
		(x.IsStuck ? (int)TimeSeriesStatus.Stuck : 0) +
		(x.IsValueOutOfRange ? (int)TimeSeriesStatus.ValueOutOfRange : 0);

	protected override Expression<Func<TimeSeries, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(TimeSeries.Id):  // Id in UI is really DtId
				{
					return filter.CreateExpression((TimeSeries v) => v.Id, filter.ToString(formatProvider));
				}
			case nameof(TimeSeries.DtId):
				{
					return filter.CreateExpression((TimeSeries v) => v.DtId, filter.ToString(formatProvider));
				}
			case nameof(TimeSeries.ModelId):
				{
					return filter.CreateExpression((TimeSeries v) => v.ModelId, filter.ToString(formatProvider));
				}
			case nameof(TimeSeries.EstimatedPeriod):
				{
					return filter.CreateExpression((TimeSeries v) => v.EstimatedPeriod.TotalSeconds, filter.ToDouble(formatProvider));
				}
			case nameof(TimeSeries.MaxValue):
				{
					return filter.CreateExpression((TimeSeries v) => v.MaxValue, filter.ToDouble(formatProvider));
				}
			case nameof(TimeSeries.MinValue):
				{
					return filter.CreateExpression((TimeSeries v) => v.MinValue, filter.ToDouble(formatProvider));
				}
			case nameof(TimeSeries.AverageValue):
				{
					return filter.CreateExpression((TimeSeries v) => v.AverageValue, filter.ToDouble(formatProvider));
				}
			case nameof(TimeSeries.UnitOfMeasure):
				{
					return filter.CreateExpression((TimeSeries v) => v.UnitOfMeasure, filter.ToString(formatProvider));
				}
			case nameof(TimeSeries.TotalValuesProcessed):
				{
					return filter.CreateExpression((TimeSeries v) => v.TotalValuesProcessed, filter.ToInt64(formatProvider));
				}
			case "Status":
				{
					int scoreValue = filter.ToInt32(formatProvider);
										
					if (scoreValue == 0)
					{
						return filter.CreateExpression(score, filter.ToInt32(formatProvider));
					}

					return filter.CreateExpression(score, scoreValue, isFlag: true);
				}
			case nameof(TimeSeries.IsOffline):
				{
					return filter.CreateExpression((TimeSeries v) => v.IsOffline, filter.ToBoolean(formatProvider));
				}
			case nameof(TimeSeries.IsPeriodOutOfRange):
				{
					return filter.CreateExpression((TimeSeries v) => v.IsPeriodOutOfRange, filter.ToBoolean(formatProvider));
				}
			case nameof(TimeSeries.IsStuck):
				{
					return filter.CreateExpression((TimeSeries v) => v.IsStuck, filter.ToBoolean(formatProvider));
				}
			case nameof(TimeSeries.IsValueOutOfRange):
				{
					return filter.CreateExpression((TimeSeries v) => v.IsValueOutOfRange, filter.ToBoolean(formatProvider));
				}
			default:
				{
					return null;
				}
		}
	}

	protected override IQueryable<TimeSeries> ApplySort(IQueryable<TimeSeries> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<TimeSeries>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				case nameof(TimeSeries.DtId):
					{
						result = AddSort(queryable, result!, first, x => x.DtId, sortSpecification.sort)
							.ThenBy(x => x.Id);
						break;
					}
				case nameof(TimeSeries.ModelId):
					{
						result = AddSort(queryable, result!, first, x => x.ModelId, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.EstimatedPeriod):
					{
						result = AddSort(queryable, result!, first, x => x.EstimatedPeriod, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.TrendInterval):
					{
						result = AddSort(queryable, result!, first, x => x.TrendInterval, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.IsOffline):
					{
						result = AddSort(queryable, result!, first, x => x.IsOffline, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.IsPeriodOutOfRange):
					{
						result = AddSort(queryable, result!, first, x => x.IsPeriodOutOfRange, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.IsStuck):
					{
						result = AddSort(queryable, result!, first, x => x.IsStuck, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.IsValueOutOfRange):
					{
						result = AddSort(queryable, result!, first, x => x.IsValueOutOfRange, sortSpecification.sort);
						break;
					}
				case "Status":  // calculated value
					{
						result = AddSort(queryable, result!, first, score, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.MaxValue):
					{
						result = AddSort(queryable, result!, first, x => x.MaxValue, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.MinValue):
					{
						result = AddSort(queryable, result!, first, x => x.MinValue, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.AverageValue):
					{
						result = AddSort(queryable, result!, first, x => x.AverageValue, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.UnitOfMeasure):
					{
						result = AddSort(queryable, result!, first, x => x.UnitOfMeasure, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.TotalValuesProcessed):
					{
						result = AddSort(queryable, result!, first, x => x.TotalValuesProcessed, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.LastSeen):
					{
						result = AddSort(queryable, result!, first, x => x.LastSeen, sortSpecification.sort);
						break;
					}
				case nameof(TimeSeries.Latency):
					{
						result = AddSort(queryable, result!, first, x => x.Latency, sortSpecification.sort);
						break;
					}
				default:
				case nameof(TimeSeries.Id):
					{
						result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort)
							.ThenBy(x => x.DtId);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	public async Task RemoveAll()
	{
		logger.LogInformation($"Remove all timeseries");
		await ExecuteAsync(async () =>
		{
			await this.rulesContext.Database.ExecuteSqlInterpolatedAsync($"TRUNCATE TABLE [TimeSeries]");
		});

	}

	public Task<TimeSeries?> GetByTwinId(string twinId)
	{
		return GetQueryable().FirstOrDefaultAsync(x => x.DtId == twinId);
	}
}

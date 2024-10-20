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
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Repository;

public interface IRepositoryLogEntry : IRepositoryBase<LogEntry>
{
	/// <summary>
	/// Limits log rows for a certain progress id
	/// </summary>
	Task LimitLogsForProgressId(string progressId, int limit);

	/// <summary>
	/// Deletes logs before a specified date
	/// </summary>
	Task DeleteLogsBefore(DateTime date);

	/// <summary>
	/// Pruns all logs
	/// </summary>
	Task PruneLogs();
}

/// <summary>
/// A repository for <see cref="LogEntry"/>
/// </summary>
public class RepositoryLogEntry : RepositoryBase<LogEntry>, IRepositoryLogEntry
{
	public RepositoryLogEntry(
			IDbContextFactory<RulesContext> dbContextFactory,
			RulesContext rulesContext,
			WillowEnvironmentId willowEnvironment,
			IMemoryCache memoryCache,
			IEpochTracker epochTracker,
			ILogger<RepositoryLogEntry> logger,
			IOptions<CustomerOptions> customerOptions)
		: base(dbContextFactory, rulesContext, rulesContext.Logs, willowEnvironment, memoryCache, epochTracker, logger, customerOptions)
	{
	}

	protected override Expression<Func<LogEntry, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
		{
			case nameof(LogEntry.TimeStamp):
				{
					return filter.CreateExpression((LogEntry v) => v.Id, filter.ToString(formatProvider));
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
	public override IQueryable<LogEntry> WithArrays(IQueryable<LogEntry> input)
	{
		return input;//.Include(a => a.Occurrences);
	}

	/// <inheritdoc />
	protected override IQueryable<LogEntry> ApplySort(IQueryable<LogEntry> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
		IOrderedQueryable<LogEntry>? result = null;
		foreach (var sortSpecification in sortSpecifications)
		{
			switch (sortSpecification.field)
			{
				default:
				case nameof(LogEntry.TimeStamp):
					{
						result = AddSort(queryable, result!, first, x => x.TimeStamp, sortSpecification.sort);
						break;
					}
			}
			first = false;
		}
		return result ?? queryable;
	}

	public async Task LimitLogsForProgressId(string progressId, int limit)
	{
		using (var timed = logger.TimeOperationOver(TimeSpan.FromSeconds(30), "Pruning logs for progress id {id}", progressId))
		{
			var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(60));

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
							.ExecuteSqlInterpolatedAsync($@"
								SET NOCOUNT OFF;
								DECLARE @Rows INT
								SELECT @Rows = COUNT (*) FROM [Logs] WHERE ProgressId = {progressId}
								IF @Rows > {limit}
								BEGIN
									DELETE T FROM
										(SELECT top ({batchSize}) *
											FROM [Logs] logs
											WHERE ProgressId = {progressId}
											order by TimeStamp) T
									select @@ROWCOUNT
								END
								ELSE
								BEGIN
								 select 0
								END");

						count += currentCount;

						if (count > 0)
						{
							throttledLogger.LogDebug("{count} logs deleted so far {progressId}", count, progressId);
						}
					}
					while (currentCount > 0);

					if (count > 0)
					{
						logger.LogInformation("Deleted {count} logs {progressId}", count, progressId);
					}
				});
			}
			catch (OperationCanceledException ex)
			{
				logger.LogError(ex, "Delete {progressId} logs cancelled", progressId);
				throw;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to delete logs for progressId {progressId}", progressId);
				throw;
			}
		}
	}

	public async Task DeleteLogsBefore(DateTime date)
	{
		using (var timed = logger.TimeOperationOver(TimeSpan.FromSeconds(30), "Pruning logs before {date}", date))
		{
			var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(60));

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
							.ExecuteSqlInterpolatedAsync($@"
								SET NOCOUNT OFF;
								DELETE TOP ({batchSize}) l FROM [Logs] l
								WHERE l.[TimeStamp] < {date}
								select @@ROWCOUNT");

						count += currentCount;

						if (count > 0)
						{
							throttledLogger.LogInformation("{count} logs deleted so far {date}", count, date);
						}
					}
					while (currentCount > 0);

					if (count > 0)
					{
						logger.LogInformation("Deleted {count} logs before {date}", count, date);
					}
				});
			}
			catch (OperationCanceledException ex)
			{
				logger.LogError(ex, "Delete {date} logs cancelled", date);
				throw;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to delete logs for data {date}", date);
				throw;
			}
		}
	}

	public async Task PruneLogs()
	{
		await DeleteLogsBefore(DateTime.UtcNow.AddDays(-7));//prune all logs
		await LimitLogsForProgressId(Progress.RealtimeExecutionId, 1000);//dont let realtime logs grow too big in the table
	}
}

using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.DTO;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Repository;

/// <summary>
/// Methods that all repositories implement
/// </summary>
public interface IRepositoryBase<T> : IBaseRepository
{
	/// <summary>
	/// Bulk merge using EFCore.BulkExtensions
	/// </summary>
	Task BulkMerge(IList<T> items, BulkConfig? config = null, bool updateOnly = false, CancellationToken cancellationToken = default);

	/// <summary>
	/// Bulk delete a set of items
	/// </summary>
	Task BulkDelete(IList<T> items, bool updateCache = true, CancellationToken cancellationToken = default);

	/// <summary>
	/// Count matching
	/// </summary>
	Task<int> Count(Expression<Func<T, bool>> queryExpression);

	/// <summary>
	/// Any matching
	/// </summary>
	Task<bool> Any(Expression<Func<T, bool>> queryExpression);

	/// <summary>
	/// Delete one item
	/// </summary>
	Task DeleteOne(T item, bool updateCache = true);

	/// <summary>
	/// Flush any queue'd bulk writes
	/// </summary>
	Task FlushQueue(int batchSize = 4000, bool updateCache = true, BulkConfig? config = null, bool updateOnly = false);

	/// <summary>
	/// Get items matching query
	/// </summary>
	Task<IEnumerable<T>> Get(Expression<Func<T, bool>>? queryExpression = null);

	/// <summary>
	/// Get queryable for Type
	/// </summary>
	IQueryable<T> GetQueryable();

	/// <summary>
	/// Get items matching query ascending
	/// </summary>
	Task<IEnumerable<T>> GetAscending<U>(Expression<Func<T, bool>> queryExpression, Expression<Func<T, U>> keySelector, int? limit = null);

	/// <summary>
	/// Get items matching query descending
	/// </summary>
	Task<IEnumerable<T>> GetDescending<U>(Expression<Func<T, bool>> queryExpression, Expression<Func<T, U>> keySelector, int? limit = null);

	/// <summary>
	/// Get all matching items of type T from the repository
	/// </summary>
	Task<Batch<T>> GetAll(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<T, bool>>? whereExpression = null,
		int? page = null,
		int? take = null);

	/// <summary>
	/// Get all records
	/// </summary>
	IAsyncEnumerable<T> GetAll(Expression<Func<T, bool>>? whereExpression = null);

	/// <summary>
	/// Get one item by id
	/// </summary>
	Task<T?> GetOne(string id, bool updateCache = true);

	/// <summary>
	/// Upsert using a queue to take advantage of bulk merge
	/// </summary>
	Task QueueWrite(T item, int queueSize = 4000, int batchSize = 4000, bool updateCache = true, BulkConfig? config = null, bool updateOnly = false);

	/// <summary>
	/// Upsert one item to the database
	/// </summary>
	Task<T> UpsertOne(T value, bool updateCache = true, CancellationToken cancellationToken = default);

	/// <summary>
	/// Upsert one item using a queue and deduplicating on same Id
	/// </summary>
	Task UpsertOneUnique(T value, CancellationToken cancellationToken = default);
}


/// <summary>
/// Base-level repository
/// </summary>
public interface IBaseRepository
{
	/// <summary>
	/// Gets the cache hit ratio
	/// </summary>
	double CacheHitRatio { get; }
}

/// <summary>
/// Tracks the database Epoch (which increments on each write to handle cache invalidation)
/// </summary>
public interface IEpochTracker
{
	int CacheEpoch { get; }

	/// <summary>
	/// Invalidates the in memory cache of any realtional data
	/// </summary>
	void InvalidateCache();
}

/// <summary>
/// Tracks database epoch for memory cache invalidation
/// </summary>/
/// <remarks>
/// This is a singleton across the whole app so that anyone can invalidate the cache for all uses at once
/// </remarks>
public class EpochTracker : IEpochTracker
{
	/// <summary>
	/// Cache epoch changes on every write to ensure reads do not get stale data
	/// </summary>
	/// <remarks>
	/// Static because repositories have a per-request lifetime but the memory cache lives on.
	/// Could be per table but let's keep it simple for now, updates (apart from Insights and RuleMetdata)
	/// will be infrequent.
	/// </remarks>
	private static int cacheEpoch = 0;

	/// <summary>
	/// Gets the cache epoch for invalidating memory caches
	/// </summary>
	public int CacheEpoch => cacheEpoch;

	/// <summary>
	/// Invalidates the cache by bumping the epoch counter used for memory cache keys
	/// </summary>
	/// <remarks>
	/// Call this when a ServiceBus message is received from the Processor, or on each local write
	/// </remarks>
	public void InvalidateCache()
	{
		Interlocked.Increment(ref cacheEpoch);
	}
}


/// <summary>
/// Non-generic base class for counters shared across all repositories
/// </summary>
public abstract class RepositoryBase : IBaseRepository
{
	/// <summary>
	/// Cache hit counter
	/// </summary>
	protected static ulong cacheHits = 0;

	/// <summary>
	/// Cache miss counter
	/// </summary>
	protected static ulong cacheMisses = 0;

	/// <summary>
	/// Epoch tracker
	/// </summary>
	protected readonly IEpochTracker epochTracker;

	/// <inheritdoc/>
	public double CacheHitRatio
	{
		get
		{
			return cacheHits / (cacheHits + cacheMisses + 1.0);
		}
	}

	/// <summary>
	/// Creates a new <see cref="RepositoryBase" />
	/// </summary>
	protected RepositoryBase(IEpochTracker epochTracker)
	{
		this.epochTracker = epochTracker ?? throw new ArgumentNullException(nameof(epochTracker));
	}

	/// <summary>
	/// Invalidates the cache when a ServiceBus message is received from the Processor
	/// or when a bulk write happens.
	/// </summary>
	public virtual void InvalidateCache()
	{
		epochTracker.InvalidateCache();
	}


	/// <summary>
	/// Retry policy for three retries with exponential backoff
	/// </summary>
	public static AsyncRetryPolicy RetryPolicy = Policy
		.Handle<Exception>()
		.WaitAndRetryAsync(5, retryAttempt =>
			TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
		, (Exception e, TimeSpan retry, int count, Context c) =>
		{
			if (c.TryGetValue("logger", out var l) && l is ILogger logger)
			{
				if (e is SqlException sqlException &&
				sqlException.Message.Contains("error: 40 - Could not open a connection to SQL Server"))
				{
					logger.LogInformation(e, "Attempting to reconnect to database, retry attempt : {count}", count);
				}
			}
		});

}

/// <summary>
/// Base repository class
/// </summary>
public abstract class RepositoryBase<T> : RepositoryBase, IRepositoryBase<T>, IDisposable
	where T : class, IId
{

	/// <summary>
	/// Creates a new <see cref="RepositoryBase{T}"/>
	/// </summary>
	public RepositoryBase(
		IDbContextFactory<RulesContext> dbContextFactory,
		RulesContext rulesContext,
		DbSet<T> dbSet,
		WillowEnvironmentId willowEnvironmentId,
		IMemoryCache memoryCache,
		IEpochTracker epochTracker,
		ILogger logger,
		IOptions<CustomerOptions> customerOptions)
		: base(epochTracker)
	{
		if (customerOptions is null)
		{
			throw new ArgumentNullException(nameof(customerOptions));
		}

		this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
		this.rulesContext = rulesContext ?? throw new ArgumentNullException(nameof(rulesContext));
		this.dbSet = dbSet ?? throw new ArgumentNullException(nameof(dbSet));
		this.willowEnvironmentId = willowEnvironmentId;
		this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.throttledLogger = logger.Throttle(TimeSpan.FromSeconds(5));
		cacheExpiration = customerOptions.Value.SQL.CacheExpiration;
	}

	protected readonly IDbContextFactory<RulesContext> dbContextFactory;
	protected readonly RulesContext rulesContext;
	protected readonly DbSet<T> dbSet;
	protected readonly WillowEnvironmentId willowEnvironmentId;
	protected readonly IMemoryCache memoryCache;
	protected readonly ILogger logger;
	protected readonly ILogger throttledLogger;
	protected readonly TimeSpan cacheExpiration;

	/// <summary>
	/// Adds the includes to the dbset
	/// </summary>
	/// <returns></returns>
	public abstract IQueryable<T> WithArrays(IQueryable<T> input);

	protected IOrderedQueryable<T> AddSortAscending<U>(IQueryable<T> queryable,
		IOrderedQueryable<T> orderedQueryable,
		bool first, Expression<Func<T, U>> fieldSelector)
	{
		return queryable.AddSortAscending(orderedQueryable, first, fieldSelector);
	}

	protected IOrderedQueryable<T> AddSortDescending<U>(IQueryable<T> queryable,
		IOrderedQueryable<T> orderedQueryable,
		bool first, Expression<Func<T, U>> fieldSelector)
	{
		return queryable.AddSortDescending(orderedQueryable, first, fieldSelector);
	}

	protected IOrderedQueryable<T> AddSort<U>(IQueryable<T> queryable,
		IOrderedQueryable<T> orderedQueryable,
		bool first, Expression<Func<T, U>> fieldSelector, string direction)
	{
		return queryable.AddSort(orderedQueryable, first, fieldSelector, direction);
	}

	/// <summary>
	/// Apply the sort specification
	/// </summary>
	protected abstract IQueryable<T> ApplySort(IQueryable<T> queryable, SortSpecificationDto[] sortSpecifications);

	/// <summary>
	/// Apply a filter specification.
	/// </summary>
	/// <remarks>
	/// Return value can be null if a filter specification goes across different entity types;
	/// </remarks>
	protected abstract Expression<Func<T, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider);

	protected static ConstantExpressionVisitor constantExpressionVisitor = new ConstantExpressionVisitor();

	/// <inheritdoc/>
	public virtual async Task<Batch<T>> GetAll(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<T, bool>>? whereExpression = null,
		int? page = null,
		int? take = null)
	{
		return await ExecuteAsync(async () =>
		{
			whereExpression = BuildWhereClause(filterSpecifications, whereExpression);

			var queryable = whereExpression is null ? GetQueryable() : GetQueryable().Where(whereExpression);

			queryable = ApplyBatchParams(queryable, sortSpecifications);

			// logger.LogInformation("GetAll epoch={epoch} `{where}` after `{after}` sort `{sort}`",
			// 	epochTracker.CacheEpoch,
			// 	whereExpression?.ToString() ?? "",
			// 	afterExpression?.ToString() ?? "",
			// 	string.Join("+", sortSpecifications.Select(x => $"{x.field}:{x.sort}")));

			var batch = await GetOrCreateBatch(queryable, (v) => v.Id, page, take);

			return batch;
		});
	}

	/// <inheritdoc/>
	public virtual async IAsyncEnumerable<T> GetAll(Expression<Func<T, bool>>? whereExpression = null)
	{
		var queryable = WithArrays(GetQueryable());

		if (whereExpression is not null)
		{
			queryable = queryable.Where(whereExpression);
		}

		var (enumerator, movedNext) = await ExecuteAsync(
			async () =>
			{
				var asyncEnumerable = queryable.AsAsyncEnumerable();
				var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();
				(IAsyncEnumerator<T> Enumerator, bool MovedNext) result = (asyncEnumerator, await asyncEnumerator.MoveNextAsync());

				return result;
			});

		try
		{
			if (!movedNext)
			{
				yield break;
			}

			do
			{
				yield return enumerator.Current;

			} while (await enumerator.MoveNextAsync());

		}
		finally
		{
			await enumerator.DisposeAsync();
		}
	}

	// Cache Id needs to include environment, type and Id because same Id may be used
	// for more than one type of object, e.g. RuleInstance and RuleInstanceMetadata
	protected string GetCacheKey(string id) => this.willowEnvironmentId + "_" +
		epochTracker.CacheEpoch + "_" + typeof(T).Name + "_" + id;

	/// <summary>
	/// Removes one item from the cache
	/// </summary>
	protected void InvalidateOne(string id)
	{
		memoryCache.Remove(GetCacheKey(id));
	}

	/// <inheritdoc/>
	public async Task<T?> GetOne(string id, bool updateCache = true)
	{
		// TODO: Use GetOrreateAsync
		// T? item2 = await memoryCache.GetOrCreateAsync(id, async (c) =>
		// {
		// 	await retryPolicy.ExecuteAsync(async () =>
		// 	{
		// 		return await WithArrays(dbSet).FirstOrDefaultAsync(x => x.Id == id);
		// 	});

		// 	return null;
		// });

		if (updateCache)
		{
			if (memoryCache.TryGetValue<T>(GetCacheKey(id), out var value))
			{
				cacheHits++;
				return value;
			}

			cacheMisses++;
		}

		T? item = null;

		await ExecuteAsync(async () =>
		{
			item = await WithArrays(GetQueryableForSingle()).FirstOrDefaultAsync(x => x.Id == id);
		});

		if (updateCache)
		{
			SetCache(item);
		}

		return item;
	}

	/// <inheritdoc/>
	public async Task DeleteOne(T item, bool updateCache = true)
	{
		if (updateCache)
		{
			memoryCache.Remove(GetCacheKey(item.Id));
		}

		await ExecuteAsync(async () =>
		{
			await BulkDelete(new List<T> { item }, updateCache: updateCache);
		});
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<T>> Get(Expression<Func<T, bool>>? queryExpression = null)
	{
		return await ExecuteAsync(async () =>
		{
			if (queryExpression is not null)
			{
				return await WithArrays(GetQueryable()).Where(queryExpression).ToListAsync();
			}

			return await WithArrays(GetQueryable()).ToListAsync();
		});
	}

	/// <inheritdoc/>
	public async Task<int> Count(Expression<Func<T, bool>> queryExpression)
	{
		return await ExecuteAsync(async () =>
		{
			return await GetQueryable().CountAsync(queryExpression);
		});
	}

	/// <inheritdoc/>
	public async Task<bool> Any(Expression<Func<T, bool>> queryExpression)
	{
		return await ExecuteAsync(async () =>
		{
			return await GetQueryable().AnyAsync(queryExpression);
		});
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<T>> GetAscending<U>(Expression<Func<T, bool>> queryExpression, Expression<Func<T, U>> keySelector, int? limit = null)
	{
		return await ExecuteAsync(async () =>
		{
			IQueryable<T> expresion = WithArrays(GetQueryable()).Where(queryExpression).OrderBy(keySelector);

			if (limit is not null)
			{
				expresion = expresion.Take(limit.Value);
			}

			var result = await expresion.ToListAsync();

			return result;
		});
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<T>> GetDescending<U>(Expression<Func<T, bool>> queryExpression, Expression<Func<T, U>> keySelector, int? limit = null)
	{
		return await ExecuteAsync(async () =>
		{
			IQueryable<T> expresion = WithArrays(GetQueryable()).Where(queryExpression).OrderByDescending(keySelector);

			if (limit is not null)
			{
				expresion = expresion.Take(limit.Value);
			}

			var result = await expresion.ToListAsync();

			return result;
		});
	}

	protected async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action)
	{
		return await RetryPolicy.ExecuteAsync(async (c, t) =>
		{
			return await action();
		}, new Dictionary<string, object>() { ["logger"] = logger }, CancellationToken.None);
	}

	protected async Task ExecuteAsync(Func<Task> action)
	{
		await RetryPolicy.ExecuteAsync(async (c, t) =>
		{
			await action();
		}, new Dictionary<string, object>() { ["logger"] = logger }, CancellationToken.None);
	}

	/// <inheritdoc/>
	public virtual async Task<T> UpsertOne(T value, bool updateCache = true, CancellationToken cancellationToken = default)
	{
		if (updateCache)
		{
			SetCache(value);
		}

		await ExecuteAsync(async () =>
		{
			await Upsert(value);
			return value;
		});

		return default(T)!;
	}

	/// <inheritdoc/>
	public virtual async Task UpsertOneUnique(T value, CancellationToken cancellationToken = default)
	{
		// TODO: Fix QueuedWriter to work with new approach to DI for Repositories
		// go direct for now, queued writer needs fixing to work with EF
		await Upsert(value);
		// this.queuedWriter.UpsertOneUnique(value, cancellationToken);
		// Really need to increment only after it completes the write (!) TODO
		InvalidateCache();
		// return Task.CompletedTask;
	}

	/// <summary>
	/// Gets the Schema Table name for the DbSet.
	/// </summary>
	public string GetTableSchemaName()
	{
		var m = this.rulesContext.Model.FindEntityType(typeof(T));
		return m!.GetSchemaQualifiedTableName()!;
	}

	public async Task BulkMerge(IList<T> items, BulkConfig? config = null, bool updateOnly = false, CancellationToken cancellationToken = default)
	{
		// No need for retry here, all calls to this method are retried
		using (var rc = await dbContextFactory.CreateDbContextAsync(cancellationToken))
		{
			// Increase timeout for bulk merge operations, especially on ActorState
			rc.Database.SetCommandTimeout(120);

			try
			{
				// This is still slow, takes 45-60s for 500 x ActorState
				await BulkInsertOrUpdateAsync(rc, items, config, updateOnly: updateOnly, cancellationToken: cancellationToken);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to bulk merge {type}", typeof(T).Name);
				throw;
			}
		}
	}

	protected virtual Task BulkInsertOrUpdateAsync(RulesContext rc, IList<T> items, BulkConfig? config = null, bool updateOnly = false, CancellationToken cancellationToken = default)
	{
		config ??= new BulkConfig();
		//disable EXISTS/EXCEPT logic for MERGE statement otherwise SQL will ignore case-only changes
		config.OmitClauseExistsExcept = true;

		if (updateOnly)
		{
			return rc.BulkUpdateAsync<T>(items, config, cancellationToken: cancellationToken);
		}
		else
		{
			return rc.BulkInsertOrUpdateAsync<T>(items, config, cancellationToken: cancellationToken);
		}
	}

	public virtual async Task BulkDelete(IList<T> items, bool updateCache = true, CancellationToken cancellationToken = default)
	{
		using (var rc = await dbContextFactory.CreateDbContextAsync(cancellationToken))
		{
			await BulkDeleteAsync(rc, items, cancellationToken);

			if (updateCache)
			{
				this.InvalidateCache();
			}
		}
	}

	protected virtual Task BulkDeleteAsync(RulesContext rc, IList<T> items, CancellationToken cancellationToken = default)
	{
		return rc.BulkDeleteAsync<T>(items, cancellationToken: cancellationToken);
	}

	readonly ConcurrentQueue<T> queue = new();

	public virtual async Task QueueWrite(T item, int queueSize = 4000, int batchSize = 4000, bool updateCache = true, BulkConfig? config = null, bool updateOnly = false)
	{
		if (string.IsNullOrEmpty(item.Id)) throw new ArgumentNullException(nameof(item.Id));

		if (updateCache)
		{
			SetCache(item);
		}

		queue.Enqueue(item);

		if (queue.Count > queueSize)
		{
			await FlushQueue(batchSize, updateCache, config: config, updateOnly: updateOnly);
		}
	}

	public async Task FlushQueue(int batchSize = 4000, bool updateCache = true, BulkConfig? config = null, bool updateOnly = false)
	{
		int count = queue.Count;
		if (count == 0) return;

		using (var timedLogger = logger.TimeOperationOver(TimeSpan.FromSeconds(30), "Flush {type} queue {count:N0} items", typeof(T).Name, count))
		{
			// Ensure only one flush operation happens at a time
			// BulkMerge does not like updating the same Id twice in one operation
			// Must deduplicate like this
			// Comparer can't be case sensitive as Sql is not case sensitive on Primary Keys
			Dictionary<string, T> items = new(StringComparer.OrdinalIgnoreCase);
			int wrote = 0;

			try
			{
				while (queue.TryDequeue(out var value))
				{
					if (string.IsNullOrWhiteSpace(value.Id))
					{
						logger.LogWarning($"Empty Id for {JsonConvert.SerializeObject(value)}");
						continue;
					}

					// Last write to dictionary wins
					items[value.Id] = value;

					if (items.Count() == batchSize)
					{
						await ExecuteAsync(async () =>
						{
							await this.BulkMerge(items.Values.ToList(), config: config, updateOnly: updateOnly);
						});
						wrote += items.Count();

						// And start a new buffer
						items = new();
					}
				}

				if (items.Any())  // remainder
				{
					await ExecuteAsync(async () =>
					{
						await this.BulkMerge(items.Values.ToList(), config: config, updateOnly: updateOnly);
					});
					wrote += items.Count();
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "FlushQueue exception {type} ({wrote}/{count})", typeof(T).Name, wrote, count);
				logger.LogWarning(ex.Message);
			}
			if (updateCache)
			{
				this.InvalidateCache();
			}
		}
	}

	/// <inheritdoc />
	public virtual void Dispose()
	{
		this.FlushQueue().GetAwaiter().GetResult();
	}

	/// <summary>
	/// Is this Http status code a success code (same implementation as core uses in HttpClient)
	/// </summary>
	private static bool IsSuccessStatusCode(HttpStatusCode statusCode) => ((int)statusCode >= 200) && ((int)statusCode <= 299);


	// See https://askgif.com/blog/303/best-way-to-update-or-replace-entities-in-entity-framework-6/

	private async Task Upsert(T item, bool updateCache = true)
	{
		try
		{
			await ExecuteAsync(async () =>
			{
				// Using Flexlabs to do Upsert 'cause EF doesn't have Upsert
				await BulkMerge(new List<T> { item });
			});
		}
		catch (Exception ex)
		{
			//  System.InvalidOperationException: A second operation was started on this context before a previous operation completed. This is usually caused by different threads concurrently using the same instance of DbContext. For more information on how to avoid threading issues with DbContext, see https://go.microsoft.com/fwlink/?linkid=2097913.
			logger.LogError(ex, $"Failed to upsert {typeof(T).Name} item");
		}

		if (updateCache)
		{
			this.InvalidateCache();
		}
	}

	/// <summary>
	/// Builds an expression for filtering records after a start point
	/// </summary>
	protected Expression<Func<T, bool>> AddAfter(Expression<Func<T, bool>> current,
		bool ascending,
		Expression<Func<T, bool>> greater,
		Expression<Func<T, bool>> equal,
		Expression<Func<T, bool>> lesser
		)
	{
		if (ascending)
			return ExpressionExtensions.AorBandC(greater, equal, current);
		else
			return ExpressionExtensions.AorBandC(lesser, equal, current);
	}

	/// <summary>
	/// Builds an expression for filtering records after a start point
	/// </summary>
	protected Expression<Func<T, bool>> AddComparer(Expression<Func<T, bool>> current,
		bool ascending,
		Expression<Func<T, int>> comparer)
	{
		Expression<Func<T, bool>> greater =
			(Expression<Func<T, bool>>)Expression.Lambda(
					Expression.GreaterThan(comparer.Body, Expression.Constant(0)), comparer.Parameters[0]);
		Expression<Func<T, bool>> equal =
			(Expression<Func<T, bool>>)Expression.Lambda(
					Expression.Equal(comparer.Body, Expression.Constant(0)), comparer.Parameters[0]);
		Expression<Func<T, bool>> lesser =
			(Expression<Func<T, bool>>)Expression.Lambda(
					Expression.LessThan(comparer.Body, Expression.Constant(0)), comparer.Parameters[0]);

		return AddAfter(current, ascending, greater, equal, lesser);
	}

	/// <summary>
	/// Get the initial comparer for a sort sequence
	/// </summary>
	protected Expression<Func<T, bool>> GetInitial(SortSpecificationDto[] sortSpecifications, T value)
	{
		var id = value.Id;
		bool lastDirection = (sortSpecifications.LastOrDefault()?.sort.ToUpperInvariant() ?? "ASC") == "ASC";
		Expression<Func<T, bool>>? result =
			lastDirection ? (T x) => x.Id.CompareTo(id) > 0 : (T x) => x.Id.CompareTo(id) < 0
			;
		return result;
	}

	public virtual IQueryable<T> GetQueryable()
	{
		return dbSet;
	}

	protected virtual IQueryable<T> GetQueryableForSingle()
	{
		return dbSet;
	}

	protected async Task<Batch<TBatch>> GetOrCreateBatch<TBatch>(
		IQueryable<TBatch> queryable,
		Func<TBatch, string> getId,
		int? page = null,
		int? take = null)
	{
		//EF's querystring extension generates enough detail for the cache key
		string queryString = queryable.ToQueryString();

		//logger.LogInformation("{query}", queryable.ToQueryString());

		List<TBatch> items = new();
		var skip = 0;
		int total = 0;

		await ExecuteAsync(async () =>
		{
			total = await queryable.CountAsync();

			queryable = queryable.Page(page, take, out skip);

			items = await queryable.ToListAsync();
		});

		int countBefore = skip;
		int countAfter = total - skip;
		var last = items.LastOrDefault();
		string next = last is null ? "" : getId(last);

		return new Batch<TBatch>(queryString, countBefore, countAfter, total, items, next);
	}

	protected IQueryable<T> ApplyBatchParams(IQueryable<T> queryable,
		SortSpecificationDto[] sortSpecifications)
	{
		//logger.LogInformation("Before sort applied: {query}", queryable.ToQueryString());

		queryable = ApplySort(queryable, sortSpecifications);

		// Include statements
		queryable = WithArrays(queryable);

		return queryable;
	}

	protected Expression<Func<T, bool>>? BuildWhereClause(FilterSpecificationDto[] filterSpecifications, Expression<Func<T, bool>>? whereExpression = null)
	{
		Expression<Func<T, bool>>? filterExpression = null;

		foreach (var filterSpecification in filterSpecifications)
		{
			var expression = GetExpression(filterSpecification, null);

			filterExpression = filterSpecification.ApplyLogicalOperator(filterExpression, expression);
		}

		if (filterExpression is not null)
		{
			if (whereExpression is not null)
			{
				return whereExpression.And(filterExpression);
			}

			return filterExpression;
		}

		return whereExpression;
	}

	private void SetCache(T? value)
	{
		if (value is not null)
		{
			memoryCache.Set(GetCacheKey(value.Id), value, absoluteExpirationRelativeToNow: cacheExpiration);
		}
	}
}

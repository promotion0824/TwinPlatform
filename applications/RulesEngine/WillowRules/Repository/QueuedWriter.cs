// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Polly;
// using Polly.Retry;
// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using Willow.Rules.Model;
// using Willow.Rules.Services;
// using System.Linq;

// namespace Willow.Rules.Repository;

// /// <summary>
// /// Queue'd writer on separate thread for writing to the database with deduplication and throttling
// /// </summary>
// public interface IQueuedWriter<T> : IDisposable
// {
// 	void UpsertOneUnique(T value, CancellationToken cancellationToken = default);
// }

// /// <summary>
// /// Queue'd writer on separate thread for writing to the database with deduplication and throttling
// /// </summary>
// public class QueuedWriter<T> : IQueuedWriter<T>, IDisposable
// 		where T : class, IId
// {
// 	/// <summary>
// 	/// Shutsdown when disposed
// 	/// </summary>
// 	private CancellationTokenSource shutdownTokenSource = new CancellationTokenSource();

// 	/// <summary>
// 	/// A queue that keeps just the most recent of each T item by Id
// 	/// </summary>
// 	private UniqueQueue<T> uniqueWriterQueue;

// 	/// <summary>
// 	/// Lazy writer with deduplicated writes
// 	/// </summary>
// 	private Lazy<Task> writer;
// 	private readonly RulesContext rulesContext;
// 	private readonly DbSet<T> dbSet;
// 	private readonly ILogger<QueuedWriter<T>> logger;

// 	/// <summary>
// 	/// Coallesce writes for the same Id that happen within this window of time
// 	/// </summary>
// 	/// <remarks>
// 	/// This saves on database hits during frequent fast updates to the same entry
// 	/// </remarks>
// 	private TimeSpan delayWrites;

// 	public QueuedWriter(RulesContext rulesContext, DbSet<T> dbSet,
// 		ILogger<QueuedWriter<T>> logger, TimeSpan? delayWrites = null)
// 	{
// 		this.uniqueWriterQueue = new(logger);
// 		this.shutdownTokenSource = new CancellationTokenSource();
// 		this.writer = new Lazy<Task>(() => writerTask(shutdownTokenSource.Token));
// 		this.rulesContext = rulesContext ?? throw new ArgumentNullException(nameof(rulesContext));
// 		this.dbSet = dbSet ?? throw new ArgumentNullException(nameof(dbSet));
// 		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
// 		this.delayWrites = delayWrites ?? TimeSpan.Zero;
// 	}

// 	/// <inheritdoc/>
// 	public virtual void UpsertOneUnique(T value, CancellationToken cancellationToken = default)
// 	{
// 		var wt = this.writer.Value;  // ensure writer is running
// 		this.uniqueWriterQueue.Enqueue(value);
// 	}

// 	public virtual void Dispose()
// 	{
// 		shutdownTokenSource.Cancel();
// 	}

// 	private static AsyncRetryPolicy retryPolicy = Policy
// 		.Handle<Exception>()  // TODO: Specific SQL Exception
// 		.WaitAndRetryAsync(3, retryAttempt =>
// 			TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
// 		);

// 	/// <summary>
// 	/// Task that defers writes and removes duplicate writes
// 	/// </summary>
// 	/// <remarks>
// 	/// This is mostly for insights and any other frequently updated item where we need to limit duplicate writes
// 	/// </remarks>
// 	private Task writerTask(CancellationToken cancellationToken)
// 	{
// 		var writerTask = Task.Run(async () =>
// 		{
// 			try
// 			{
// 				while (!cancellationToken.IsCancellationRequested)
// 				{
// 					try
// 					{
// 						// DEQUEUE
// 						(DateTimeOffset queued, T item) = this.uniqueWriterQueue.Dequeue(cancellationToken);   // blocking call

// 						var delta = delayWrites - (DateTimeOffset.Now - queued);
// 						if (delta > TimeSpan.Zero)
// 						{
// 							// If less than a minute in queue, wait up to the minute mark
// 							// this slows writes on frequently updated items to once per minute
// 							// e.g. insights will be written to the database at most once per minute
// 							await Task.Delay(delta, cancellationToken);
// 						}

// 						await QueuedWriter<T>.retryPolicy.ExecuteAsync(async () =>
// 						{
// 							await Upsert(item);
// 						});
// 					}
// 					catch (Exception ex)
// 					{
// 						logger.LogError(ex, "Failed to upsert queued item");
// 					}
// 				}
// 			}
// 			catch (OperationCanceledException)
// 			{
// 				// do nothing, expected
// 			}
// 			catch (Exception ex)
// 			{
// 				logger.LogError(ex, "Writer task could not write document");
// 			}
// 		});

// 		return writerTask;
// 	}

// 	private async Task Upsert(T item)
// 	{
// 		var existing = await dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == item.Id);

// 		if (existing is null)
// 		{
// 			dbSet.Add(item);
// 			await rulesContext.SaveChangesAsync();
// 		}
// 		else
// 		{
// 			// Horrible hack, I hate EF!
// 			if (existing is Insight insight && item is Insight newInsight)
// 			{
// 				// Reload it with Occurrences
// 				insight = ((await dbSet.Include("Occurrences").FirstOrDefaultAsync(x => x.Id == item.Id)) as Insight)!;

// 				insight.Invocations = newInsight.Invocations;
// 				insight.LastUpdated = newInsight.LastUpdated;
// 				insight.Reliability = newInsight.Reliability;
// 				insight.Text = newInsight.Text;

// 				//insight.Occurrences = newInsight.Occurrences;
// 				foreach (var insightOccurrence in newInsight.Occurrences)
// 				{
// 					var match = insight.Occurrences.FirstOrDefault(x => x.Id == insightOccurrence.Id);

// 					if (match is null)
// 					{
// 						insight.Occurrences.Add(insightOccurrence);
// 					}
// 					else if (!match.Equals(insightOccurrence))
// 					{
// 						match.Started = insightOccurrence.Started;
// 						match.Ended = insightOccurrence.Ended;
// 						match.Text = insightOccurrence.Text;
// 					}
// 				}
// 			}
// 			else
// 			{
// 				// Replace the item in the tracked set with the new one
// 				rulesContext.ChangeTracker.TrackGraph(item, _ => { });
// 			}

// 			await rulesContext.SaveChangesAsync();
// 		}
// 	}
// }

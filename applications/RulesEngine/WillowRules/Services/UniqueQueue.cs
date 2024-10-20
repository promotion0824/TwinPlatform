using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;
using Willow.Rules.Repository;

namespace Willow.Rules.Services
{
	/// <summary>
	/// Unique queue is used on writers to ensure we only write last value of each object pending a write
	/// </summary>
	public class UniqueQueue<T> where T : IId
	{
		private struct PendingItem
		{
			public DateTimeOffset queued;
			public DateTimeOffset updated;
			public T value;

			/// <summary>
			/// Creates a new pending item
			/// </summary>
			public PendingItem(T item) : this()
			{
				this.value = item;
				this.queued = DateTimeOffset.Now;
				this.updated = DateTimeOffset.Now;
			}

			/// <summary>
			/// Update an existing pending item
			/// </summary>
			public PendingItem(T item, PendingItem existing) : this(item)
			{
				this.value = item;
				this.queued = existing.queued;
				this.updated = DateTimeOffset.Now;
			}
		}

		/// <summary>
		/// Items already waiting are updated in the dictionary
		/// </summary>
		private ConcurrentDictionary<string, PendingItem> dictionary;

		/// <summary>
		/// Items not currently in the dictionary are added to the queue
		/// </summary>
		private BlockingCollection<string> queue;

		/// <summary>
		/// Logger
		/// </summary>
		private readonly ILogger logger;

		/// <summary>
		/// Creates a new <see cref="UniqueQueue{T}"/>
		/// </summary>
		public UniqueQueue(ILogger logger)
		{
			dictionary = new();
			queue = new();
			this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
		}

		private object lockObject = new object();

		/// <summary>
		/// Enqueue an item, replacing an existing one of the same id
		/// </summary>
		public void Enqueue(T item)
		{
			lock (lockObject)
			{
				bool existing = dictionary.ContainsKey(item.Id);
				var itemNew = dictionary.AddOrUpdate(item.Id,
					(s) => new PendingItem(item),
					(s, existing) => new PendingItem(item, existing));
				if (!existing)
				{
					queue.Add(item.Id);
				}

				ReportSignificantQueueCountChanges();
			}
		}

		/// <summary>
		/// Logs when the queue increases size by a factor of 10 or decreases by a factor of 5
		/// </summary>
		private void ReportSignificantQueueCountChanges()
		{
			// Report excessive queue lengths using hysteresis on a multiplier
			if (queue.Count > 10 * (reportedCount + 1) && queue.Count > 100)
			{
				logger.LogWarning($"Queue<{typeof(T).Name}> has {queue.Count} items pending");
				reportedCount = queue.Count;
			}
			else if (queue.Count * 5 < reportedCount)
			{
				logger.LogInformation($"Queue<{typeof(T).Name}> has {queue.Count} items pending");
				reportedCount = queue.Count;
			}
		}

		long reportedCount = 0;

		/// <summary>
		/// Remove an item from the queue as a blocking operation
		/// </summary>
		/// <exception cref="System.OperationCanceledException"></exception>
		public (DateTimeOffset queued, T value) Dequeue(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				string id = queue.Take(cancellationToken);   // blocking

				if (dictionary.TryRemove(id, out PendingItem pending))
				{
					ReportSignificantQueueCountChanges();
					return (pending.queued, pending.value);
				}
			}

			throw new System.OperationCanceledException("Unique queue cancellation");
		}
	}
}
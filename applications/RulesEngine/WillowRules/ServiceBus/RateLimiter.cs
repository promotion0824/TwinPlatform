using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Willow.ServiceBus;

/// <summary>
/// Rate limiter for logging exceptions
/// </summary>
public class RateLimiter
{
	private Queue<(DateTimeOffset time, string key)> queue;
	private readonly int keep;
	private readonly int minutes;

	public RateLimiter(int keep, int minutes)
	{
		this.keep = keep;
		this.minutes = minutes;
		this.queue = new(keep + 1);
	}

	/// <summary>
	/// Perform action with same key only once every N minutes
	/// </summary>
	public void Limit(string key, Action action, ILogger logger)
	{
		var now = DateTimeOffset.Now;
		if (queue.All(x => x.key != key || now < x.time.AddMinutes(minutes)))
		{
			//logger.LogDebug($"Limited keys: {string.Join(",", queue.Select(x => $"{x.key} {now - x.time}"))}");

			action();

			queue.Enqueue((now, key));
			if (queue.Count > this.keep)
			{
				queue.Dequeue();
			}
		}
	}

	/// <summary>
	/// Perform action with same key only once every N minutes
	/// </summary>
	public async Task Limit(string key, Func<Task> action)
	{
		if (queue.All(x => x.key != key ||
			DateTimeOffset.Now < x.time.AddMinutes(minutes)))
		{
			await action();

			queue.Enqueue((DateTimeOffset.Now, key));
			if (queue.Count > this.keep)
			{
				queue.Dequeue();
			}
		}
	}

}
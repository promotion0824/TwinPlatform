using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Willow.Rules;

/// <summary>
/// Channel extension methods
/// </summary>
/// <remarks>
/// See https://deniskyashif.com/2019/12/08/csharp-channels-part-1/
/// </remarks>
public static class ChannelExtensions
{
	/// <summary>
	/// Limit the queue length in each channel
	/// </summary>
	public const int MaxQueueLength = 50;

	private static readonly BoundedChannelOptions boundedChannelOptions = new BoundedChannelOptions(MaxQueueLength) { SingleReader = true };

	/// <summary>
	/// Creates a bounded channel from an IAsyncEnumerable and copies all the data to it
	/// </summary>
	public static (Channel<T> channel, Task task) CreateChannel<T>(this IAsyncEnumerable<T> input, int bound = 100, CancellationToken cancellationToken = default)
	{
		var channel = Channel.CreateBounded<T>(bound);

		var producer = Task.Run(async () =>
		{
			try
			{
				await foreach (var item in input)
				{
					await channel.Writer.WriteAsync(item, cancellationToken);
				}
			}
			finally
			{
				channel.Writer.Complete();
			}
		}, cancellationToken);

		return (channel, producer);
	}

	/// <summary>
	/// Split a channel into subchannels using a key to ensure same-key processed by same channel
	/// which can help prevent file conflict errors
	/// </summary>
	public static IList<ChannelReader<T>> Split<T>(this ChannelReader<T> ch, IEnumerable<string> keys, Func<T, string> keyVaue, CancellationToken cancellationToken = default)
	{
		var outputs = new Dictionary<string, Channel<T>>();

		foreach (var key in keys)
		{
			outputs.Add(key, Channel.CreateBounded<T>(boundedChannelOptions));
		}

		Task.Run(async () =>
		{
			try
			{
				await foreach (var item in ch.ReadAllAsync(cancellationToken).ConfigureAwait(false))
				{
					var key = keyVaue(item);
					await outputs[key].Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
				}
			}
			finally
			{
				foreach (var ch in outputs)
					ch.Value.Writer.Complete();
			}
		});

		return outputs.Select(ch => ch.Value.Reader).ToArray();
	}

	/// <summary>
	/// Split a channel into subchannels using a key to ensure same-key processed by same channel
	/// which can help prevent file conflict errors
	/// </summary>
	public static IList<ChannelReader<T>> Split<T, U>(this ChannelReader<T> ch, int n, Func<T, U> key, CancellationToken cancellationToken = default)
		where U : notnull
	{
		var outputs = new Channel<T>[n];

		for (int i = 0; i < n; i++)
			outputs[i] = Channel.CreateBounded<T>(boundedChannelOptions);

		Task.Run(async () =>
		{
			try
			{
				await foreach (var item in ch.ReadAllAsync(cancellationToken).ConfigureAwait(false))
				{
					// Consistent hash so the same item ends up on the same writer and doesn't conflict
					// very subtle difference between Math.Abs and & 0x7FFFFFFFF (hint one value is still -ve)
					int index = (key(item).GetHashCode() & 0x7fffffff) % n;
					await outputs[index].Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
				}
			}
			finally
			{
				foreach (var ch in outputs)
					ch.Writer.Complete();
			}
		}, cancellationToken);

		return outputs.Select(ch => ch.Reader).ToArray();
	}

	/// <summary>
	/// Merges split channels back to a single channel
	/// </summary>
	public static ChannelReader<T> Merge<T>(CancellationToken cancellationToken = default, params ChannelReader<T>[] inputs)
	{
		var output = Channel.CreateBounded<T>(boundedChannelOptions);

		Task.Run(async () =>
		{
			try
			{
				async Task Redirect(ChannelReader<T> input)
				{
					await foreach (var item in input.ReadAllAsync(cancellationToken).ConfigureAwait(false))
						await output.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
				}

				await Task.WhenAll(inputs.Select(i => Redirect(i)).ToArray());
			}
			finally
			{
				output.Writer.Complete();
			}
		}, cancellationToken);

		return output;
	}

	/// <summary>
	/// Merges split channels back to a single channel
	/// </summary>
	public static ChannelReader<T> Merge<T>(this IEnumerable<ChannelReader<T>> inputs, CancellationToken cancellationToken = default)
	{
		var output = Channel.CreateBounded<T>(boundedChannelOptions);

		Task.Run(async () =>
		{
			try
			{
				async Task Redirect(ChannelReader<T> input)
				{
					await foreach (var item in input.ReadAllAsync(cancellationToken).ConfigureAwait(false))
						await output.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
				}

				await Task.WhenAll(inputs.Select(i => Redirect(i)).ToArray());
			}
			finally
			{
				output.Writer.Complete();
			}
		}, cancellationToken);

		return output;
	}

	/// <summary>
	/// Merges split channels back to a single channel when the channel contains IEnumerables
	/// </summary>
	public static ChannelReader<T> MergeMany<T>(this IEnumerable<ChannelReader<IEnumerable<T>>> inputs, CancellationToken cancellationToken = default)
	{
		var output = Channel.CreateBounded<T>(boundedChannelOptions);

		Task.Run(async () =>
		{
			try
			{
				async Task Redirect(ChannelReader<IEnumerable<T>> input)
				{
					await foreach (var enumeration in input.ReadAllAsync(cancellationToken).ConfigureAwait(false))
						foreach (var item in enumeration)
							await output.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
				}

				await Task.WhenAll(inputs.Select(i => Redirect(i)).ToArray());
			}
			finally
			{
				output.Writer.Complete();
			}
		}, cancellationToken);

		return output;
	}

	/// <summary>
	/// Where on an input stream to an output stream
	/// </summary>
	public static ChannelReader<T> Where<T>(this ChannelReader<T> inputChannel, Func<T, bool> filter, CancellationToken cancellationToken = default)
	{
		var output = Channel.CreateBounded<T>(boundedChannelOptions);

		Task.Run(async () =>
		{
			try
			{
				await foreach (T input in inputChannel.ReadAllAsync(cancellationToken).ConfigureAwait(false))
				{
					if (filter(input))
					{
						await output.Writer.WriteAsync(input, cancellationToken).ConfigureAwait(false);
					}
				}
			}
			finally
			{
				output.Writer.Complete();
			}
		}, cancellationToken);

		return output;
	}

	/// <summary>
	/// Transform an input stream to an output stream
	/// </summary>
	public static ChannelReader<U> Transform<T, U>(this ChannelReader<T> inputChannel, Func<T, U> transformation, CancellationToken cancellationToken = default)
	{
		var output = Channel.CreateBounded<U>(boundedChannelOptions);

		Task.Run(async () =>
		{
			try
			{
				await foreach (T input in inputChannel.ReadAllAsync(cancellationToken).ConfigureAwait(false))
				{
					await output.Writer.WriteAsync(transformation(input), cancellationToken).ConfigureAwait(false);
				}
			}
			finally
			{
				output.Writer.Complete();
			}
		}, cancellationToken);

		return output;
	}

	/// <summary>
	/// Transform an input stream
	/// </summary>
	public static Task ExecuteAsync<T>(this ChannelReader<T> inputChannel, Func<T, Task> transformation, CancellationToken cancellationToken = default)
	{
		return Task.Run(async () =>
		{
			await foreach (T input in inputChannel.ReadAllAsync(cancellationToken).ConfigureAwait(false))
			{
				await transformation(input);
			}
		}, cancellationToken);
	}


	/// <summary>
	/// Condenses similar timestamps
	/// </summary>
	public static ChannelReader<T> CondenseTimeStampsAsync<T>(this ChannelReader<T> inputChannel,
		Func<T, DateTime> datetimeGetter,
		Func<T, string> idGetter,
		TimeSpan holdTime,
		ILogger? logger = null,
		CancellationToken cancellationToken = default)
	{
		var output = Channel.CreateBounded<T>(boundedChannelOptions);

		var condenser = new ConcurrentDictionary<string, T>();
		var queue = new Queue<T>();

		Task.Run(async () =>
		{
			try
			{
				await foreach (T input in inputChannel.ReadAllAsync(cancellationToken).ConfigureAwait(false))
				{
					DateTime now = datetimeGetter(input);
					string id = idGetter(input);  // e.g. the rule instance id

					condenser.AddOrUpdate(id, input, (k, old) => input);
					queue.Enqueue(input);

					// While we have items at the top of the queue that are ready to release
					// remove them but dedupe if they've already been pulled
					while (queue.TryPeek(out T? first) && datetimeGetter(first).Add(holdTime) < now)
					{
						T top = queue.Dequeue();
						string topId = idGetter(top);

						// only one will make it, any dupes will be removed
						// and we will have the latest version
						if (condenser.TryRemove(topId, out var t))
						{
							await output.Writer.WriteAsync(t, cancellationToken).ConfigureAwait(false);
						}
					}

				}

				logger?.LogDebug("Draining queue");

				// And now drain the queue, still with deduplication
				while (queue.TryDequeue(out T? first))
				{
					if (condenser.TryRemove(idGetter(first), out var t))  // only one will make it, any dupes will be removed
					{
						await output.Writer.WriteAsync(t, cancellationToken).ConfigureAwait(false);
					}
				}

				logger?.LogDebug("Complete");

				output.Writer.Complete();
			}
			catch (Exception ex)
			{
				logger?.LogError(ex, "Error in condenser");
			}
		});

		return output;
	}

	/// <summary>
	/// Transform an input stream to an output stream
	/// </summary>
	public static ChannelReader<U> TransformAsync<T, U>(this ChannelReader<T> inputChannel, Func<T, Task<U>> transformation, CancellationToken cancellationToken = default)
	{
		var output = Channel.CreateBounded<U>(boundedChannelOptions);

		Task.Run(async () =>
		{
			try
			{
				await foreach (T input in inputChannel.ReadAllAsync(cancellationToken).ConfigureAwait(false))
				{
					var result = await transformation(input);

					await output.Writer.WriteAsync(result, cancellationToken).ConfigureAwait(false);
				}
			}
			finally
			{
				output.Writer.Complete();
			}
		});

		return output;
	}

	/// <summary>
	/// Transform an input stream to an output stream
	/// </summary>
	public static ChannelReader<U> TransformManyAsync<T, U>(this ChannelReader<T> inputChannel, Func<T, IAsyncEnumerable<U>> transformation, CancellationToken cancellationToken = default)
	{
		var output = Channel.CreateBounded<U>(boundedChannelOptions);

		Task.Run(async () =>
		{
			try
			{
				await foreach (T input in inputChannel.ReadAllAsync(cancellationToken).ConfigureAwait(false))
				{
					await foreach (U transformed in transformation(input))
					{
						await output.Writer.WriteAsync(transformed, cancellationToken).ConfigureAwait(false);
					}
				}
			}
			finally
			{
				output.Writer.Complete();
			}
		});

		return output;
	}

	/// <summary>
	/// Extends ReadAllAsync with a delay on the reader
	/// </summary>
	public static async IAsyncEnumerable<T> ReadAllAsync<T>(this ChannelReader<T> inputChannel, TimeSpan timeout, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (T input in inputChannel.ReadAllAsync(cancellationToken).ConfigureAwait(false))
		{
			await Task.Delay(timeout);

			yield return input;
		}
	}

	/// <summary>
	/// Awaits channel messages and capable of processing variable size batches to a maxBatchSize
	/// </summary>
	public static async Task<List<T>> ReadMultipleAsync<T>(this ChannelReader<T> inputChannel, int maxBatchSize, CancellationToken cancellationToken = default)
	{
		await inputChannel.WaitToReadAsync(cancellationToken);

		var batch = new List<T>();

		while (batch.Count < maxBatchSize && inputChannel.TryRead(out var message))
		{
			batch.Add(message);
		}

		return batch;
	}
}

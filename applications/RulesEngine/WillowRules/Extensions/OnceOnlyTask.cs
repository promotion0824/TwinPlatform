using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WillowRules.Extensions;

/// <summary>
/// Ensures things happen only once
/// </summary>
/// <remarks>
/// For example: A single web page will make multiple requests, these will cause the user
/// to be loaded multiple times as they come in simultaneously.
/// To prevent that we combine them and make only one request.
///
/// Or a user may hit refresh on a slow graph load but we only want to generate the graph once no
/// matter what.
/// </remarks>
public class OnceOnly<T>
{
	private static readonly ConcurrentDictionary<string, Lazy<Task<T>>> pendingRequests = new();
	private static readonly ConcurrentDictionary<string, Lazy<T>> pendingSyncRequests = new();

	/// <summary>
	/// Run core async code just once per key
	/// </summary>
	public static async Task<T> Execute(string key, Func<Task<T>> coreCode)
	{
		// See https://andrewlock.net/making-getoradd-on-concurrentdictionary-thread-safe-using-lazy/
		// This ensures only one Lazy is returned by the GetOrAdd method
		// which we can then get the value from and they will all be the same Task

		var lazyTask = new Lazy<Task<T>>(coreCode);

		var task = pendingRequests.GetOrAdd(key, lazyTask);

		try
		{
			// Everyone gets the same task (by key) and the function to generate the task only runs once
			var result = await task.Value;
			return result;

		}
		finally
		{
			// Assumes all the concurrency requests for this user are already waiting on the singleton task
			// If another request comes in after this point it will refetch which is no big deal, this
			// optimization is for the single page load which was loading the user four times.
			pendingRequests.TryRemove(key, out _);
		}
	}

	/// <summary>
	/// Run core sync code just once per key
	/// </summary>
	public static T Execute(string key, Func<T> coreCode)
	{
		// See https://andrewlock.net/making-getoradd-on-concurrentdictionary-thread-safe-using-lazy/
		// This ensures only one Lazy is returned by the GetOrAdd method
		// which we can then get the value from and they will all be the same Task

		var task = pendingSyncRequests.GetOrAdd(key, (userid) => new Lazy<T>(coreCode));

		// Everyone gets the same func and it runs only once
		var result = task.Value;

		// Assumes all the concurrency requests already waiting on the singleton task
		// If another request comes in after this point it will refetch which is no big deal

		pendingSyncRequests.TryRemove(key, out _);

		return result;
	}
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Willow.Rules.Cache
{
	/// <summary>
	/// A simple two-tier memory and then disk cache
	/// </summary>
	public interface IDataCache<T>
	{
		/// <summary>
		/// Get an item from the cache or create a new one and insert it
		/// </summary>
		Task<T?> GetOrCreateAsync(string willowEnvironment, string id, Func<Task<T?>> create);

		/// <summary>
		/// Add an item to the cache or update an existing one
		/// </summary>
		Task<T> AddOrUpdate(string willowEnvironment, string id, T serializableVersion);

		/// <summary>
		/// Are there any items in the cache?
		/// </summary>
		Task<bool> Any(string willowEnvironmentId);

		/// <summary>
		/// Get every item in the cache by enumerating all disk files
		/// </summary>
		IAsyncEnumerable<T> GetAll(string willowEnvironment, int maxParallelism = 40);

		/// <summary>
		/// Count by enumerating all disk files
		/// </summary>
		Task<int> Count(string willowEnvironment, int maxParallelism = 40);

		/// <summary>
		/// Try to get an item from the cache by id
		/// </summary>
		Task<(bool ok, T? result)> TryGetValue(string willowEnvironment, string id);

		/// <summary>
		/// Remove a cached item if it exists
		/// </summary>
		Task RemoveKey(string willowEnvironment, string id);

		/// <summary>
		/// Deletes all items for a given last updated date
		/// </summary>
		Task RemoveItems(string willowEnvironmentId, DateTimeOffset lastUpdated);
	}
}

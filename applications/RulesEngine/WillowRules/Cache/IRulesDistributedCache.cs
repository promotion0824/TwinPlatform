using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Willow.Rules.Cache
{
	public interface IRulesDistributedCache
	{
		/// <summary>
		/// Get All keys for a given starts with key
		/// </summary>
		IAsyncEnumerable<string> GetAllKeysAsync(string? startsWith);

		/// <summary>
		/// Count for a given starts with key
		/// </summary>
		Task<int> CountAsync(string? startsWith);

		/// <summary>
		/// Get All values for a given starts with key
		/// </summary>
		IAsyncEnumerable<byte[]> GetAllValuesAsync(string startsWith);

		/// <summary>
		/// Get a value for a given key
		/// </summary>
		Task<byte[]?> GetAsync(string key);

		/// <summary>
		/// Remove a value for a given key
		/// </summary>
		Task RemoveAsync(string key);

		/// <summary>
		/// Removes Values for any key for the given startwsith and last update date
		/// </summary>
		Task<int> RemoveAsync(string startsWith, DateTimeOffset lastUpdated);

		/// <summary>
		/// Create or update a value for a given key
		/// </summary>
		Task SetAsync(string key, byte[] binary, TimeSpan? expirationRelativeToNow);
	}
}

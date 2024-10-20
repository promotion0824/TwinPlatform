using Microsoft.Extensions.Caching.Memory;

namespace WillowRules.Extensions;

public static class CacheExtensions
{
	/// <summary>
	/// Compacts memory cache up to 75%
	/// </summary>
	public static void Compact(this IMemoryCache cache)
	{
		if (cache is MemoryCache memoryCache)
		{
			memoryCache.Compact(0.75);
		}
	}
}

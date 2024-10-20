using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Polly;
using Polly.Retry;
using Willow.Rules.Configuration;
using Willow.Rules.Logging;
using WillowRules.Extensions;

namespace Willow.Rules.Cache;


/// <summary>
/// A simple disk cache with an expiration policy and a reload policy
/// </summary>
/// <remarks>
/// In some cases we might want a disk cache like this, in others this will be a database
/// This serves as a simple Key-Value database
///
///  Directory structure is
///      cacheroot
///      then willowenvironmentid
///      then the cleaned cache name
///      then the type of the object
///      and then the object name itself broken up to ensure reasonable size directories
///
/// </remarks>
public class DataCache<T> : IDataCache<T>, IDisposable where T : notnull
{
	// Then the willow environment id (supplied as a parameter)

	/// <summary>
	/// Then the cache name
	/// </summary>
	/// <example>Models</example>
	private readonly string name;

	/// <summary>
	/// Then the type name
	/// </summary>
	private readonly string typeName;

	private string rootKey(string willowEnvironmentId) =>
		$"{willowEnvironmentId}-{name}-{typeName}";

	private readonly IMemoryCache memoryCache;
	private readonly IRulesDistributedCache rulesDistributedCache;

	private readonly ILogger logger;

	/// <summary>
	/// Implements a cache expiration
	/// </summary>
	private readonly TimeSpan maxAge;
	private readonly CachePolicy cachePolicy;
	private readonly MemoryCachePolicy memoryCachePolicy;

	/// <summary>
	/// Creates a new <see cref="DataCache{T}"/>
	/// </summary>
	public DataCache(string name,
		TimeSpan maxAge,
		CachePolicy cachePolicy,
		MemoryCachePolicy memoryCachePolicy,
		IMemoryCache memoryCache,
		IRulesDistributedCache rulesDistributedCache,
		ILoggerFactory loggerFactory)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
		}

		if (loggerFactory is null)
		{
			throw new ArgumentNullException(nameof(loggerFactory));
		}

		this.name = name;

		this.logger = loggerFactory.CreateLogger<T>();

		// Generic type - expand to avoid List'1 as a directory name
		if (typeof(T).IsGenericType)  // e.g. List<T>
		{
			var targs = typeof(T).GetGenericArguments().Select(x => x.Name); // should be just one
			this.typeName = $"{typeof(T).Name.Split('`', 2).First()} of {string.Join(" ", targs)}";
		}
		else
		{
			this.typeName = typeof(T).Name;
		}

		this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		this.rulesDistributedCache = rulesDistributedCache ?? throw new ArgumentNullException(nameof(rulesDistributedCache));
		this.maxAge = maxAge;
		this.cachePolicy = cachePolicy;
		this.memoryCachePolicy = memoryCachePolicy;
	}

	private string GetKey(string environmentId, string id)
	{
		var rootKeyValue = rootKey(environmentId);

		return $"{rootKeyValue}-{id}";
	}

	public async Task<T?> GetOrCreateAsync(string willowEnvironment, string id, Func<Task<T?>> create)
	{
		(bool ok, T? value) = await TryGetValue(willowEnvironment, id);

		if (ok && value is not null) return value;

		value = await OnceOnly<T?>.Execute(id, create);  // TBD, should this wrap the TryGetValue too? Probably.

		if (value is null) return value;
		await AddOrUpdate(willowEnvironment, id, value);
		return value;
	}

	/// <summary>
	/// Are there any objects in the cache?
	/// </summary>
	public async Task<bool> Any(string willowEnvironmentId)
	{
		var rootKeyValue = rootKey(willowEnvironmentId);

		return await rulesDistributedCache.GetAllKeysAsync(startsWith: rootKeyValue).AnyAsync();
	}

	/// <summary>
	/// Get all objects of type T stored as JSON in the subdirectory
	/// </summary>
	public async IAsyncEnumerable<T> GetAll(string willowEnvironmentId, int maxParallelism = 8)
	{
		var rootKeyValue = rootKey(willowEnvironmentId);

		await foreach (var binary in rulesDistributedCache.GetAllValuesAsync(startsWith: rootKeyValue))
		{
			var result = FromBson(binary) ?? default;
			yield return result!;
		}
	}

	/// <summary>
	/// Count all objects of type T stored as JSON in the subdirectory
	/// </summary>
	public async Task<int> Count(string willowEnvironmentId, int maxParallelism = 8)
	{
		var rootKeyValue = rootKey(willowEnvironmentId);
		int c = await rulesDistributedCache.CountAsync(startsWith: rootKeyValue);
		return c;
	}

	public Task RemoveItems(string willowEnvironmentId, DateTimeOffset lastUpdated)
	{
		var rootKeyValue = rootKey(willowEnvironmentId);
		logger.LogInformation("Removing cached items under key: {key}", rootKeyValue);
		return rulesDistributedCache.RemoveAsync(rootKeyValue, lastUpdated);
	}

	private static MemoryCacheEntryOptions memoryCacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(1));

	/// <inheritdoc />
	public async Task RemoveKey(string willowEnvironment, string id)
	{
		var key = GetKey(willowEnvironment, id);

		await rulesDistributedCache.RemoveAsync(key);

		if (this.memoryCachePolicy == MemoryCachePolicy.WithMemoryCache)
		{
			memoryCache.Remove(key);
		}
	}

	/// <summary>
	/// Policy to retry serialization in case it fails due to collection being modified
	/// </summary>
	/// <remarks>
	/// This is NOT ideal, should make it immutable again, or lock around it
	/// </remarks>
	private readonly RetryPolicy policy = Policy
		.Handle<System.InvalidOperationException>()
		.WaitAndRetry(3, retryAttempt =>
			TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
		);

	/// <inheritdoc />
	public async Task<T> AddOrUpdate(string willowEnvironment, string id, T value)
	{
		// Logs only if over the duration specified
		using (var timedOperation = logger.TimeOperationOver(TimeSpan.FromSeconds(30), "Writing {id}", id))
		{
			if (value is null) return value!;

			var key = GetKey(willowEnvironment, id);

			//a bit of a trade off here. Making keys longer will slow down cache?
			//keys going over this limit are viewing twin graphs with a large number of twin id's being posted up
			var keyHash = key.GetHashCode();

			if (key.Length > 449)
			{
				logger.LogWarning($"Could not write to cache for key '{key}' of length {key.Length}. Key limit exceeded.");
				return value;
			}

			return await OnceOnly<T>.Execute(key, async () =>
			{
				try
				{
					var bson = policy.Execute(() => ToBson(value));

					await rulesDistributedCache.SetAsync(key, bson, maxAge < TimeSpan.MaxValue ? maxAge : null);
				}
				catch (OutOfMemoryException ex)
				{
					logger.LogError(ex, "Could not write Bson for {id}, too large", id);
				}
				return value;
			});
		}
	}

	private static byte[] ToBson(T value)
	{
		using (var ms = new MemoryStream())
		using (var datawriter = new BsonDataWriter(ms))
		{
			var serializer = new JsonSerializer()
			{
				NullValueHandling = NullValueHandling.Ignore
			};

			serializer.Serialize(datawriter, value);
			return ms.ToArray();
		}
	}

	private static T? FromBson(byte[] data)
	{
		using (var ms = new MemoryStream(data))
		using (var reader = new BsonDataReader(ms))
		{
			var serializer = new JsonSerializer();
			return serializer.Deserialize<T>(reader);
		}
	}

	/// <summary>
	/// TryGetValue from memory then disk cache
	/// </summary>
	public async Task<(bool ok, T? result)> TryGetValue(string willowEnvironment, string id)
	{
		var key = GetKey(willowEnvironment, id);

		try
		{
			if (this.memoryCachePolicy == MemoryCachePolicy.WithMemoryCache)
			{
				if (memoryCache.TryGetValue<T>(key, out var value))
				{
					return (true, value);
				}
			}

			var bytes = await rulesDistributedCache.GetAsync(key);

			if (bytes != null)
			{
				var result = FromBson(bytes);

				if (result is T)
				{
					if (memoryCachePolicy == MemoryCachePolicy.WithMemoryCache)
					{
						memoryCache.Set(key, result, memoryCacheOptions);
					}

					return (true, result);
				}
			}
		}
		catch (Newtonsoft.Json.JsonSerializationException)
		{
			//logger.LogWarning("Failed to load from disk {id}", ex.Message);
		}
		catch (Newtonsoft.Json.JsonReaderException)
		{
			//logger.LogWarning("Failed to load from disk {id}", ex.Message);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, ex.Message);
		}

		return (false, default);
	}

	public void Dispose()
	{
	}

	public override string ToString()
	{
		return $"Data cache for {typeof(T).Name}";
	}
}

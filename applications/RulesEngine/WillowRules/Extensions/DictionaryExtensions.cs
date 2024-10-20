//#define USE_CACHED_VALUES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Willow.Rules;

/// <summary>
/// Some simple dictionary extensions
/// </summary>
public static class DictionaryExtensions
{
	/// <summary>
	/// Add to a dictionary of counts
	/// </summary>
	public static void TryAdd<T>(this IDictionary<string, T> dictionary, string key, T value)
	{
		dictionary[key] = value;
	}

	/// <summary>
	/// Add to a dictionary of counts
	/// </summary>
	public static void AddOrUpdate(this IDictionary<string, int> dictionary, string key)
	{
		dictionary[key] = 1 + (dictionary.TryGetValue(key, out int value) ? value : 0);
	}

	/// <summary>
	/// Add to a dictionary of lists
	/// </summary>
	public static U AddOrUpdate<S, T, U>(this IDictionary<S, U> dictionary, S key, T value)
		where U : ICollection<T>, new()
		where S : IEquatable<S>
	{
		lock (dictionary)
		{
			if (dictionary.TryGetValue(key, out var existing))
			{
				existing.Add(value);
				return existing;
			}
			else
			{
				var list = new U { value };
				dictionary.Add(key, list);
				return list;
			}
		}
	}

	/// <summary>
	/// Add to a concurrent dictionary of lists in a thread-safe manner
	/// </summary>
	public static void AddOrUpdate<T>(this ConcurrentDictionary<string, List<T>> dictionary, string key, T value)
	{
		dictionary.AddOrUpdate(key, (s) => new List<T> { value }, (s, e) => { lock (e) { e.Add(value); return e; } });
	}

}

using System;
using System.Collections.Generic;
using System.Linq;

namespace WillowRules.Extensions;

/// <summary>
/// Extension methods for use with lists
/// </summary>
public static class ListExtensions
{
	/// <summary>
	/// Checks if consecutive list items have the specified condition
	/// </summary>
	/// <remrks>
	/// Useful for checking for overlaps in time series
	/// </remarks>
	public static bool HasConsecutiveCondition<T>(this IEnumerable<T> sourceList, Func<T, T, bool> predicate)
	{
		if (sourceList.Count() < 2) return false;

		var previous = sourceList.First();
		foreach (var current in sourceList.Skip(1))
		{
			if (predicate(previous, current)) return true;
			previous = current;
		}
		return false;
	}

	/// <summary>
	/// Checks that consecutive list items do NOT have the specified condition
	/// </summary>
	/// <remrks>
	/// Useful for checking for overlaps in time series
	/// </remarks>
	public static bool HasNotConsecutiveCondition<T>(this IEnumerable<T> sourceList, Func<T, T, bool> predicate)
	{
		if (sourceList.Count() < 2) return true;

		var previous = sourceList.First();
		foreach (var current in sourceList.Skip(1))
		{
			if (!predicate(previous, current)) return false;
			previous = current;
		}
		return true;
	}
}

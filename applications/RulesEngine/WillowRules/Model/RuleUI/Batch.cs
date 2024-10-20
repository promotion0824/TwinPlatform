using System.Collections.Generic;
using System.Linq;

namespace Willow.Rules.Model;

/// <summary>
/// A batched result
/// </summary>
/// <typeparam name="T"></typeparam>
public class Batch<T>
{
	/// <summary>
	/// Query string for debugging
	/// </summary>
	public string QueryString { get; init; }

	/// <summary>
	/// Count before
	/// </summary>
	public long Before { get; set; }

	/// <summary>
	/// Count after (including items if any)
	/// </summary>
	public long After { get; set; }

	/// <summary>
	/// Total Count
	/// </summary>
	public long Total { get; set; }

	/// <summary>
	/// Items
	/// </summary>
	public T[] Items { get; set; }

	/// <summary>
	/// A reference to the next set or records after this (if any)
	/// </summary>
	public string Next { get; set; }

	/// <summary>
	/// Creates a new Batch
	/// </summary>
	public Batch(string queryString, long countBefore, long countAfter, long total, IEnumerable<T> items, string next)
	{
		this.QueryString = queryString;
		this.Before = countBefore;
		this.After = countAfter;
		this.Total = total;
		this.Items = items.ToArray();
		this.Next = next;
	}
}

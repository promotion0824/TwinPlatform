using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;

namespace RulesEngine.Web.DTO;

/// <summary>
/// A batched result
/// </summary>
/// <typeparam name="T"></typeparam>
public class BatchDto<T>
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
	public BatchDto(string queryString, long countBefore, long countAfter, long total, IEnumerable<T> items, string next)
	{
		this.QueryString = queryString;
		this.Before = countBefore;
		this.After = countAfter;
		this.Total = total;
		this.Items = items.ToArray();
		this.Next = next;
	}

	/// <summary>
	/// Creates a new Batch
	/// </summary>
	public BatchDto(IEnumerable<T> items)
        : this(items, items.Count())
	{
	}

    /// <summary>
    /// Creates a new BatchDto from a <see cref="Batch{T}"/>
    /// </summary>
    public BatchDto(IEnumerable<T> items, int count)
    {
        this.Total = count;
        this.Items = items.ToArray();
    }

    /// <summary>
    /// Creates a new BatchDto from a <see cref="Batch{T}"/>
    /// </summary>
    public BatchDto(Batch<T> batch)
		: this(batch.QueryString, batch.Before, batch.After, batch.Total, batch.Items, batch.Next)
	{
	}
}

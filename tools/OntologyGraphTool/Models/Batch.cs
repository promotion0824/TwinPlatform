namespace OntologyGraphTool.Models;

/// <summary>
/// A batched result
/// </summary>
public struct Batch<T>
{
    /// <summary>
    /// Count before
    /// </summary>
    public long Before { get; set; }

    /// <summary>
    /// Total Count
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// Items
    /// </summary>
    public T[] Items { get; set; }

    public Batch(long before, long total, ICollection<T> items)
    {
        Before = before;
        Total = total;
        Items = items.ToArray();
    }

}

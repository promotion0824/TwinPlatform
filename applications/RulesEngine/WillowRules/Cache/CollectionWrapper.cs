using System.Collections.Generic;

namespace Willow.Rules.Cache;

/// <summary>
/// BSON seems to have issues with arrays so wrap them in an object like this for caching
/// </summary>
/// <typeparam name="T"></typeparam>
public class CollectionWrapper<T>
{
	/// <summary>
	/// The items
	/// </summary>
	public List<T> Items { get; set; }

	/// <summary>
	/// Creates a new CollectionWrapper
	/// </summary>
	/// <param name="items"></param>
	public CollectionWrapper(List<T> items)
	{
		this.Items = items;
	}
}

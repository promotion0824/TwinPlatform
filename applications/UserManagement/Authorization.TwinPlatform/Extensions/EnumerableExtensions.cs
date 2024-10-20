namespace Authorization.TwinPlatform.Extensions;

/// <summary>
/// Class for defining enumberable extensions
/// </summary>
public static class EnumerableExtensions
{
	/// <summary>
	/// Method to get delta of two enumerable collections
	/// </summary>
	/// <typeparam name="T">Entity type</typeparam>
	/// <param name="values">Source Enumerable entities</param>
	/// <param name="others">Target Enumberable entities</param>
	/// <returns>Record of Add and Remove enumerable collection</returns>
	public static (IEnumerable<T> ToAdd, IEnumerable<T> ToRemove) GetDelta<T>(
		this IEnumerable<T> values,
		IEnumerable<T> others)
	{
		var toAdd = others.Except(values).ToList();
		var toRemove = values.Except(others).ToList();

		return (toAdd, toRemove);
	}
}

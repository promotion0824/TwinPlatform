using System.Collections.Generic;
using System.Threading.Tasks;

namespace WillowRules.Test;

public static class Extensions
{
	public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
	{
		foreach (var item in enumerable)
		{
			yield return await Task.FromResult(item);
		}
	}
}

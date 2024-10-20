using System;
using System.Linq;
using Willow.Rules.Model;

namespace Willow.Rules.Services;

/// <summary>
/// Service for cleaning tags
/// </summary>
public class TagService
{
	private readonly static string[] ignoreTags =
		Enumerable.Range(1, 10).Select(x => x.ToString())
		.Concat(Enumerable.Range(1, 100).Select(x => $"L{x}"))
		.Concat(Enumerable.Range(1, 100).Select(x => $"floor{x}"))
		.Concat(Enumerable.Range(1, 100).Select(x => $"zone{x}"))
		.Concat(new[] { "janitor", "womens", "mens", "north", "south", "east", "west", "retail", "fan2", "fan3", "fan4", "centralPlaza", "RSF05",
			"deviation", "lobby",
			"a", "b", "c", "d", "e"
		})
		.ToArray();

	/// <summary>
	/// Remove irrelevant tags
	/// </summary>
	/// <param name="tagstring"></param>
	/// <returns></returns>
	public string CleanTagString(string tagstring)
	{
		return string.Join(" ", tagstring.Split(' ')
			.Select(x => TagSet.replacements.TryGetValue(x, out string? replacement) ? replacement : x)
			.Where(IsValid));
	}

	/// <summary>
	/// Is this a tag we care about (ignores 1, 2, 3, L45, womens, janitor, ...
	/// </summary>
	internal bool IsValid(string key)
	{
		return !ignoreTags
			.Any(t => t.Equals(key, StringComparison.OrdinalIgnoreCase)) &&
			!TagSet.Ignores.Any(t => t.Equals(key, StringComparison.OrdinalIgnoreCase));
	}
}

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Willow.Rules.Model;

/// <summary>
/// NamedPoints extensions
/// </summary>
public static class NamedPointsExtensions
{
	/// <summary>
	/// Append a location to the pointid if ambiguous until the list is unique or all twin locations used
	/// </summary>
	public static IEnumerable<NamedPoint> ResolveAmbiguities(this IEnumerable<NamedPoint> points, bool includeBracketFormat = true)
	{
		string separator = ".";
		string prefix = includeBracketFormat ? "[" : "";
		string suffix = includeBracketFormat ? "]" : "";

		//Convert to a dictionary for easy access
		var pointIdToVariableNameMap = points.ToDictionary(p => p.Id, p => p);

		//Reset FullName for all points in case something changed else ambiguity check will not process
		foreach (var point in pointIdToVariableNameMap.Values)
		{
			point.FullName = $"{prefix}{point.VariableName}{suffix}";
		}

		bool hasMoreLocations;
		int currentIndex = 0;

		//Do-While loop for memory efficiency, performance, and to avoid stack overflow
		do
		{
			hasMoreLocations = false;
			var ambiguousKeys = GetAmbiguousKeys(pointIdToVariableNameMap);
			if (ambiguousKeys.Count == 0)
			{
				break;
			}

			//Process each ambiguous key sequentially
			foreach (var key in ambiguousKeys)
			{
				if (pointIdToVariableNameMap.TryGetValue(key, out var point))
				{
					if (point.Locations != null && point.Locations.Count > 0 && currentIndex <= point.Locations.Count - 1)
					{
						hasMoreLocations = true;
						if (currentIndex > 0)
						{
							string locationName = point.Locations[currentIndex].Name;
							point.FullName = $"{prefix}{locationName}{suffix}{separator}{point.FullName}";
						}
					}
				}
			}

			currentIndex++;
		} while (hasMoreLocations);

		//Return the modified list of points
		return pointIdToVariableNameMap.Values;
	}

	/// <summary>
	/// Find duplicates in NamedPoints
	/// </summary>
	/// <param name="dictionary"></param>
	/// <returns></returns>
	public static List<string> GetAmbiguousKeys(Dictionary<string, NamedPoint> dictionary)
	{
		var valueCounts = new Dictionary<string, List<string>>();

		// Iterate through each key-value pair in the dictionary
		foreach (var kvp in dictionary)
		{
			if (!valueCounts.TryGetValue(kvp.Value.FullName, out var value))
			{
				value = [];
				valueCounts[kvp.Value.FullName] = value;
			}

			value.Add(kvp.Key);
		}

		var ambiguousKeys = valueCounts
			.Where(pair => pair.Value.Count > 1)
			.SelectMany(pair => pair.Value)
			.ToList();

		return ambiguousKeys;
	}
}

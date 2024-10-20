using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;
using WillowRules.Extensions;

namespace Willow.Rules.Web;

/// <summary>
/// Extensions on twin classes
/// </summary>
public static class TwinExtensions
{
    /// <summary>
    /// Group location lookup
    /// </summary>
    /// <param name="locations"></param>
    /// <returns></returns>
    public static Dictionary<string, string> GroupLocationsByModel(this IEnumerable<TwinLocation> locations)
    {
        return locations
               .GroupBy(l => l.ModelId.TrimModelId())
               .ToDictionary(
                   g => g.Key,
                   g => string.Join(", ", g.Select(l => l.Name))
               );
    }
}
